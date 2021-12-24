#r "bin/Debug/netstandard2.0/SimpleTemplateProvider.dll"

type MyTemplate = SimpleTemplateProvider.Template<"<div>{{hole1}}</div>">

let templated = MyTemplate("a").Value

printfn "Result: %A" templated
