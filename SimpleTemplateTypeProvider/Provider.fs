﻿namespace SimpleTemplateTypeProvider

open ProviderImplementation.ProvidedTypes
open FSharp.Quotations
open FSharp.Core.CompilerServices
open System.Reflection
open System.IO
open System.Text.RegularExpressions
open System.Text
open System.Diagnostics

type Debug =
    [<Conditional("DEBUG")>]
    static member print a = printfn $"Provider-Debug: {a}"

module TemplateImplementation =

    let HoleRegex = Regex(@"{{(\w+)}}", RegexOptions.Compiled)

    let getMatchValue (m: Match) = m.Groups[1].Value.Trim()

    let parseHoles template =
        HoleRegex.Matches(template)
        |> Seq.cast
        |> Seq.map getMatchValue
        |> Seq.distinct
    
    let replaceHoles template (valuesMap: Map<string, string>) =
        HoleRegex.Replace(template, fun m ->
            match valuesMap.TryGetValue(getMatchValue m) with
            | true, value -> value
            | false, _ -> failwith $"Found a hole without a value ({m}).")

/// Actual type used behind the erased Template.
type Templated(template: string, holeNames: string[], holeValues: string[]) =
    let valuesMap = (holeNames, holeValues) ||> Array.zip |> Map.ofArray
    /// Gets the templated value, that is,
    /// the template with its holes filled with the provided values.
    member val Value: string = TemplateImplementation.replaceHoles template valuesMap

type TemplateSource =
    | Auto = 0
    | Inline = 1
    | File = 2

[<TypeProvider>]
type TemplatingProvider (config: TypeProviderConfig) as this =
    inherit TypeProviderForNamespaces(config)

    let ns = "SimpleTemplateTypeProvider"
    let ass = Assembly.GetExecutingAssembly()

    do try
        let templateType = ProvidedTypeDefinition(ass, ns, "Template", Some typeof<obj>)
        templateType.AddXmlDoc("""Prepares a template.
Usage:

    let MyTemplate = Template<"<p>{{Hole}}</p>">
    let filledTemplate = MyTemplate("value")

A path to a template file can also be provided instead of the template value.""")

        let filePathOrStringParam = ProvidedStaticParameter("filePathOrString", typeof<string>) 
        filePathOrStringParam.AddXmlDoc("Template value, or path to a template file.")

        // The default value has to be an int to be consistent with the way the type provider
        // deals with enum static parameters (we receive them as ints...)
        let sourceParam = ProvidedStaticParameter("source", typeof<TemplateSource>, int TemplateSource.Auto)

        let setupWatcher fullPath =
            let watcher = new FileSystemWatcher(
                Path = Path.GetDirectoryName fullPath,
                Filter = Path.GetFileName fullPath,
                NotifyFilter = (NotifyFilters.FileName
                    ||| NotifyFilters.LastWrite
                    ||| NotifyFilters.Size
                    ||| NotifyFilters.DirectoryName
                    ||| NotifyFilters.Security
                    ||| NotifyFilters.Attributes
                    ||| NotifyFilters.CreationTime),
                EnableRaisingEvents = true)

            let onChange e =
                Debug.print $"File {fullPath} has changed. Invalidating type provider..."
                this.Invalidate()
                watcher.Dispose()

            watcher.Changed.Add(onChange)
            watcher.Renamed.Add(onChange)
            watcher.Deleted.Add(onChange)
            watcher.Created.Add(onChange)

        templateType.DefineStaticParameters(
            [ filePathOrStringParam; sourceParam ],
            fun typeName staticParams ->
                match staticParams with
                // source is an int because apparently the type provider does not
                // support enums very well and gives us int values instead.
                | [| :? string as filePathOrString; :? int as source |] ->

                    let source = enum<TemplateSource>(source)
                    Debug.print $"filePathOrString={filePathOrString}"
                    Debug.print $"source={source}"

                    let fullPath = Path.Combine(config.ResolutionFolder, filePathOrString)

                    let template =
                        let loadFile () =
                            setupWatcher fullPath
                            File.ReadAllText(fullPath, Encoding.UTF8)
                        match source with
                        | TemplateSource.Inline -> filePathOrString
                        | TemplateSource.File -> loadFile ()
                        | TemplateSource.Auto ->
                            if filePathOrString.Contains("<") then filePathOrString
                            else loadFile ()
                        | _ -> failwith $"Unexpected enum value for {nameof(TemplateSource)}: {source}"

                    Debug.print $"template={template}"

                    let holes =
                        TemplateImplementation.parseHoles template
                        |> Array.ofSeq

                    Debug.print $"holes=%A{holes}"

                    let parameterizedType = ProvidedTypeDefinition(ass, ns, typeName, Some typeof<Templated>)

                    let ctor =
                        ProvidedConstructor(
                            [ for hole in holes -> ProvidedParameter(hole, typeof<string>) ],
                            fun args ->
                                let argValues = Expr.NewArray(typeof<string>, args)
                                <@@ Templated(template, holes, %%argValues) @@>)

                    parameterizedType.AddMember(ctor)
                    parameterizedType

                | x -> failwith $"Unexpected static parameters value: %A{x}"
            )
        this.AddNamespace(ns, [templateType])
        with ex ->
            // Output the full exception, with the stack etc.
            failwith $"{ex}"

[<TypeProviderAssembly>]
do ()