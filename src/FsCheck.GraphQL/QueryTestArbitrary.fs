namespace FsCheck.GraphQL

open GraphQL.Types
open FsCheck
open Models
open System

[<AbstractClass>]
type QueryTestArbitrary<'TGraphQuery when 'TGraphQuery :> FieldType>() =
    let createGen name fields resolver =
        fields
        |> Seq.map (fun f -> Fields.fieldWithProperties f resolver 0)
        |> Gen.subListOf
        |> Gen.where (fun x -> x.Length > 0)
        |> Gen.map (fun x -> { name = name; argument = None; fields = x })

    let createGenWithArgs (query: 'TGraphQuery) fields (fetchArgs: seq<string> -> Gen<obj>) resolver =
        let genArgs() = 
            query.Arguments 
            |> Seq.map (fun x -> x.Name)
            |> fetchArgs
            |> Gen.map Arguments.get

        let genFields() =
            fields
            |> Seq.map (fun f -> Fields.fieldWithProperties f resolver 0)
            |> Gen.subListOf
            |> Gen.where (fun x -> x.Length > 0)

        gen {
            let! a = genArgs()
            let! f = genFields()
            return { name = query.Name; argument = Some a; fields = f }
        }

    static member shrink = Shrinker.shrink

    member private this.createArb (resolve: System.Type -> obj) (fetchArgs: (seq<string> -> Gen<obj>) option) =
        let query: 'TGraphQuery = downcast (resolve typeof<'TGraphQuery>): 'TGraphQuery
        let queryType = Types.resolveQueryType query.Type
        let graphType: IComplexGraphType = downcast (resolve (queryType)): IComplexGraphType
        let gen =
            match fetchArgs with
            | Some f ->
                match (Arguments.hasArguments query.Arguments) with
                | true -> createGenWithArgs query graphType.Fields f resolve
                | false -> 
                    raise ((sprintf "FetchArguments function should be defined for a type if it has some arguments, type: %s, arguments: %A" query.Name query.Arguments) |> ArgumentException)
            | None -> createGen query.Name graphType.Fields resolve

        gen |> Arb.fromGen

    member this.build (resolve: System.Type -> obj) =
        this.createArb resolve None

    member this.buildWithArgs (resolve: System.Type -> obj) (fetchArgs: seq<string> -> Gen<obj>) =
        this.createArb resolve (Some fetchArgs)