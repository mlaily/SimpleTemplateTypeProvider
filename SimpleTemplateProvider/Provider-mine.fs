namespace SimpleTemplateProvider

open ProviderImplementation.ProvidedTypes
open FSharp.Quotations
open FSharp.Linq.RuntimeHelpers
open FSharp.Core.CompilerServices
open System.Reflection
open System.IO
open System.Collections
open System

///// Represents a template whose holes can be filled.
///// This is the actual type used by the provided Template type, after erasure.
//type TemplateInstance(template) =
//    member _.GetHoles() = [] : seq<Hole>

[<TypeProvider>]
type TemplatingProvider (config: TypeProviderConfig) as this =
    inherit TypeProviderForNamespaces(config)

    let ns = "SimpleTemplateProvider"
    let asm = Assembly.GetExecutingAssembly()

    // todo: parse file name or direct value (match on < or not)
    let loadTemplate filePathOrString =
        //Path.Combine(config.ResolutionFolder, filePathOrString)
        "todo ${arg}"

    // todo
    let parseHoles template =
        [ "arg1"; "arg2" ]

    let entryPointType = ProvidedTypeDefinition(asm, ns, "Template", Some typeof<obj>)

    do entryPointType.DefineStaticParameters(
        [ ProvidedStaticParameter("filePathOrString", typeof<string>) ],
        fun typeName [| :? string as filePathOrString |] ->

            let template = loadTemplate filePathOrString

            let holes = parseHoles template

            let fillHoles template (args: Generic.IDictionary<string, string>) = ()

            // Using the static parameter yields the following type parameterized with the parameter:
            // TODO: instead of using a string as the inner state, we could pre-construct an optimized representation ready to fill...
            let theType = ProvidedTypeDefinition(asm, ns, typeName, Some typeof<obj>)
            let ctor = ProvidedConstructor([], fun _ -> <@@ template :> obj @@>)
            theType.AddMember(ctor)

            // Add erased members on the provided type:
            let holeParameters = [ for hole in holes -> ProvidedParameter(hole, typeof<string>) ]
            let fillMeth = ProvidedMethod("Fill", holeParameters, typeof<string>,
                invokeCode = fun args ->
                    try
                        //let result = 
                        //(%%args[0]:string)
                        //let holesValues = List.zip holes (args |> List.skip 1) |> dict
                        //let stringList = args |> List.map (fun x -> Expr.Cast<string>(x) :> Expr)
                        //let listExpr = Expr.NewArray(typeof<string>, stringList)
                        <@@ 
                            //let mutable template = %%args[0]:string
                            //for i, hole in holes |> List.mapi (fun i x -> i, x) do
                            //    let arg = args[i]
                            //    template <- template.Replace(hole, (%%arg:string))
                            //template
                        @@>
                    with | ex -> failwith (ex.ToString()))
            theType.AddMember(fillMeth)

            theType)

        //let ctor = ProvidedConstructor(
        //    [
        //        ProvidedParameter("Hole1", typeof<string>)
        //    ], invokeCode = fun args ->
        //        <@@
        //            let x = %%args[0] : string
        //            $"y internal state {x}" :> obj
        //        @@>)
        //myType.AddMember(ctor)

        //let resultProp = ProvidedProperty("Result", typeof<string>, getterCode = fun args -> <@@ (%%(args[0]) :> obj) :?> string @@>)
        //myType.AddMember(resultProp)

    do this.AddNamespace(ns, [entryPointType])

[<TypeProviderAssembly>]
do ()