# Simple Template Type Provider
Inspired by WebSharper and Bolero templates, but way simpler.

[![Nuget](https://img.shields.io/nuget/v/SimpleTemplateTypeProvider)](https://www.nuget.org/packages/SimpleTemplateTypeProvider)


Usage:

```fsharp
#r "SimpleTemplateTypeProvider.dll"

type MyTemplate = SimpleTemplateTypeProvider.Template<"""<div style="{{style}}">{{content}}</div>""">

let templated = MyTemplate("color: red", "message").Value // <div style="color: red">message</div>
```

The provider supports two static parameters:
- `filePathOrString`, a string that is either an inline template as above, or a template file path.
- `source`, a `TemplateSource` enum choice between `Auto`, `Inline`, or `File` allowing to explicitly specify how the `filePathOrString` parameter should be interpreted.
  - `Auto`: default value. If the string contains a `<` it is interpreted as a template. Otherwise, it is interpreted as a file path.
  - `Inline`: forces `filePathOrString` to be interpreted as an inline template.
  - `File`: forces `filePathOrString` to be interpreted as a template file path.

Example:
```fsharp
type MyTemplate = Template<"This is not Html: {{hole}}", source=TemplateSource.Inline>
type FileTemplate = Template<"shared/template.htm">
```
