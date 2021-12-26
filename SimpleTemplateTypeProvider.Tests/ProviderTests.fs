module ProviderTests

open Expecto
open SimpleTemplateTypeProvider

type NoHoleInline = Template<"noHole", source=TemplateSource.Inline>
type OneHoleInline = Template<"<p>{{hole}}</p>">
type TwoHolesInline = Template<"<p>{{hole1}}x{{hole2}}</p>">
type RepeatedHoleInline = Template<"<p>{{hole1}}x{{hole1}}</p>">

type NoHoleFile = Template<"Data/NoHole.txt">
type OneHoleFile = Template<"Data/OneHole.txt">
type TwoHolesFile = Template<"Data/TwoHoles.txt">
type RepeatedHoleFile = Template<"Data/RepeatedHole.txt">

[<Tests>]
let inlineTests =
  testList "Inline templates" [
    testCase "An inline template with no hole can be used." <| fun _ ->
        let actual = NoHoleInline().Value
        Expect.equal actual "noHole" "Failure."

    testCase "An inline template can be used" <| fun _ ->
        let actual = OneHoleInline("value").Value
        Expect.equal actual "<p>value</p>" "Failure."

    testCase "An inline template with two holes can be used" <| fun _ ->
        let actual = TwoHolesInline("a", "b").Value
        Expect.equal actual "<p>axb</p>" "Failure."

    testCase "An inline template with repeated holes can be used" <| fun _ ->
        let actual = RepeatedHoleInline("a").Value
        Expect.equal actual "<p>axa</p>" "Failure."
  ]

[<Tests>]
let fileTests =
  testList "File templates" [
    testCase "A file template with no hole can be used." <| fun _ ->
        let actual = NoHoleFile().Value
        Expect.equal actual "hello" "Failure."

    testCase "A file template can be used" <| fun _ ->
        let actual = OneHoleFile("value").Value
        Expect.equal actual "hole:value" "Failure."

    testCase "A file template with two holes can be used" <| fun _ ->
        let actual = TwoHolesFile("a", "b").Value
        Expect.equal actual "axb" "Failure."

    testCase "A file template with repeated holes can be used" <| fun _ ->
        let actual = RepeatedHoleFile("a").Value
        Expect.equal actual "a-a" "Failure."
  ]
