#r "bin/Debug/netstandard2.0/SimpleTemplateTypeProvider.dll"

type MyTemplate = SimpleTemplateTypeProvider.Template<"<div>{{hole1}}</div>">

let templated = MyTemplate("a").Value

printfn "Result: %A" templated
