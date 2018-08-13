module FieldsTestsForComplexTypes

open Xunit
open GraphQL.Types
open Swensen.Unquote
open System

[<AllowNullLiteral>]
type AnotherSource(field1: string, field2: int) =
    member this.field1 = field1
    member this.field2 = field2

type AnotherType() =
    inherit ObjectGraphType<AnotherSource>()

    do
        base.Name <- "Another"
        base.Field<StringGraphType>().Name("field1").Resolve(fun ctx -> ctx.Source.field1 :> obj) |> ignore
        base.Field<StringGraphType>().Name("field2").Resolve(fun ctx -> ctx.Source.field2 :> obj) |> ignore

[<AllowNullLiteral>]
type AnotherSimpleSource(field1: string, field2: int) =
    member this.field1 = field1
    member this.field2 = field2

type AnotherSimpleType() =
    inherit ObjectGraphType<AnotherSimpleSource>()

    do
        base.Name <- "AnotherSimple"
        base.Field<StringGraphType>().Name("simpleField1").Resolve(fun ctx -> ctx.Source.field1 :> obj) |> ignore
        base.Field<StringGraphType>().Name("simpleField2").Resolve(fun ctx -> ctx.Source.field2 :> obj) |> ignore

type SimpleOrNot() =
    inherit UnionGraphType()

    do
        base.Name <- "SimpleOrNot"
        base.Type<AnotherType>()
        base.Type<AnotherSimpleType>()

type CustomSource =
    {
        customfield1: AnotherSource
        customfield2: bool
    }

type CustomType() =
    inherit ObjectGraphType<CustomSource>()

    do
        base.Name <- "Custom"
        base.Field<AnotherType>().Name("customField1").Resolve(fun ctx -> ctx.Source.customfield1 :> obj) |> ignore
        base.Field<StringGraphType>().Name("customField2").Resolve(fun ctx -> ctx.Source.customfield2 :> obj) |> ignore

type TypeWithUnionSource =
    {
        another: AnotherSource
        anotherSimple: AnotherSimpleSource
        custom: CustomSource
    }

type TypeWithUnionType() =
    inherit ObjectGraphType<TypeWithUnionSource>()

    do
        base.Name <- "TypeWithUnion"
        base.Field<SimpleOrNot>().Name("union").Resolve(fun ctx -> if ctx.Source.another = null then ctx.Source.anotherSimple :> obj else ctx.Source.another :> obj) |> ignore
        base.Field<CustomType>().Name("custom").Resolve(fun ctx -> ctx.Source.custom:> obj) |> ignore

let resolver (t: Type): obj =
    if t = typeof<AnotherType> then new AnotherType() :> obj 
    else if t = typeof<CustomType> then new CustomType() :> obj
    else if t = typeof<AnotherSimpleType> then new AnotherSimpleType() :> obj
    else if t = typeof<TypeWithUnionType> then new TypeWithUnionType() :> obj
    else if t = typeof<SimpleOrNot> then new SimpleOrNot() :> obj
    else null

[<Fact>]
let ``unwrap when we have custom generic type should return the most concrete one`` () =
    let field = new CustomType()
    let deep = 0
    let result = Fields.fieldWithProperties (field.Fields |> Seq.head) resolver deep

    result =! "customField1 { field1 field2 }"

[<Fact>]
let ``unwrap when we have type with a union inside and custom type should return correct one`` () =
    let field = new TypeWithUnionType()
    let deep = 0
    let result = Fields.fieldWithProperties (field.Fields |> Seq.head) resolver deep

    result =! "SimpleOrNot { ... on Another { field1 field2 } ... on AnotherSimple { simpleField1 simpleField2 } }"

