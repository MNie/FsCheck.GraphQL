module QueryTestArbitraryTests

open GraphQL.Types
open GraphQL.Resolvers
open FsCheck.GraphQL
open FsCheck
open Xunit
open Swensen.Unquote

type AnotherSource =
    {
        field1: string
        field2: int
    }

type AnotherType() =
    inherit ObjectGraphType<AnotherSource>()

    do
        base.Name <- "Another"
        base.Field<StringGraphType>().Name("field1").Resolve(fun ctx -> ctx.Source.field1 :> obj) |> ignore
        base.Field<StringGraphType>().Name("field2").Resolve(fun ctx -> ctx.Source.field2 :> obj) |> ignore

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

type CustomQuery() =
    inherit FieldType()

    do
        base.Name <- "CustomQuery"
        base.Type <- typeof<CustomType>
        base.Resolver <- new FuncFieldResolver<CustomSource>((fun (_) -> { customfield1 = { field1 = "field1"; field2 = 2 }; customfield2 = true }))

type CustomQueryWithArgs() =
    inherit FieldType()

    do
        base.Name <- "CustomQueryWithArgs"
        base.Type <- typeof<CustomType>
        let intArg = QueryArgument<IntGraphType>()
        intArg.Name <- "intArg"
        let stringArg = QueryArgument<StringGraphType>()
        stringArg.Name <- "stringArg"
        base.Arguments <- QueryArguments(intArg, stringArg)
        base.Resolver <- new FuncFieldResolver<CustomSource>((fun (_) -> { customfield1 = { field1 = "field1"; field2 = 2 }; customfield2 = true }))

type CustomQueryArgs =
    {
        intArg: int
        stringArg: string
    }

let resolver (t: System.Type): obj =
    if t = typeof<AnotherType> then new AnotherType() :> obj 
    else if t = typeof<CustomType> then new CustomType() :> obj
    else if t = typeof<CustomQuery> then new CustomQuery() :> obj
    else if t = typeof<CustomQueryWithArgs> then new CustomQueryWithArgs() :> obj
    else null

type CustomQueryArb() =
    inherit QueryTestArbitrary<CustomQuery>()

    member this.create resolver = base.build resolver


type CustomQueryWithArgsArb() =
    inherit QueryTestArbitrary<CustomQueryWithArgs>()

    member this.create resolver fetchArg = this.buildWithArgs resolver fetchArg

[<Fact>]
let ``should create valid query``() =
    let arb = new CustomQueryArb()
    let allVariationsOfFields = [
        ["customField1 { field1 field2 }"]
        ["customField2"]
        ["customField1 { field1 field2 }"; "customField2"]
        ["customField2"; "customField1 { field1 field2 }"]
    ]
    let samples = 
        arb.create resolver
        |> fun x -> x.Generator
        |> Gen.sample 100 100
    let incorrectValues = (samples |> Seq.filter (fun x -> allVariationsOfFields |> Seq.contains x.fields |> not)) |> Seq.toList
    let numberOfUniqueFields = samples |> Seq.groupBy (fun x -> x.fields) |> Seq.length

    (samples |> Seq.head).name =! "CustomQuery"
    (samples |> Seq.head).argument =! None
    incorrectValues =! []
    numberOfUniqueFields >= 1


[<Fact>]
let ``should create valid query for query with args``() =
    let arb = new CustomQueryWithArgsArb()
    let allVariationsOfFields = [
        ["customField1 { field1 field2 }"]
        ["customField2"]
        ["customField1 { field1 field2 }"; "customField2"]
        ["customField2"; "customField1 { field1 field2 }"]
    ]
    let samples = 
        arb.create resolver (fun _ -> ({ intArg = 1; stringArg = "stringArg" } :> obj) |> Gen.constant)
        |> fun x -> x.Generator
        |> Gen.sample 100 100
    let incorrectValues = (samples |> Seq.filter (fun x -> allVariationsOfFields |> Seq.contains x.fields |> not)) |> Seq.toList
    let numberOfUniqueFields = samples |> Seq.groupBy (fun x -> x.fields) |> Seq.length

    (samples |> Seq.head).name =! "CustomQueryWithArgs"
    (samples |> Seq.head).argument =! Some "intArg: 1, stringArg: \"stringArg\""
    incorrectValues =! []
    numberOfUniqueFields >= 1

