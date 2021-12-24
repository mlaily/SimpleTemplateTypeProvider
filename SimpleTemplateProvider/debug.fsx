#r "bin/Debug/netstandard2.0/SimpleTemplateProvider.dll"

type MyTemplate = SimpleTemplateProvider.Template<"<div>{{hole1}}</div>">

let instance = MyTemplate("a")

printfn "holes: %A" instance.Result
