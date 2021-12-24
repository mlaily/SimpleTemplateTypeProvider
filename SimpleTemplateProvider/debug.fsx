#r "bin/Debug/netstandard2.0/SimpleTemplateProvider.dll"

type MyTemplate = Bolero.Template<"<div>${hole1}</div>">

let instance = MyTemplate()

let node = instance.hole1("test").Elt()

printfn "%A" node

// type MyTemplate = SimpleTemplateProvider.Template< "test file path" >

// let instance = MyTemplate()

// let MyTemplate = new SimpleTemplateProvider.Template< "test file path" >()


// let filled = MyTemplate.Fill("a", "b")

// printfn "%s" filled