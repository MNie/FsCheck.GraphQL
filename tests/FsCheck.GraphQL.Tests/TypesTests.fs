module Tests

open Xunit
open GraphQL.Types
open Swensen.Unquote

type CustomGenericType<'a>() =
    inherit FieldType()

[<Fact>]
let ``unwrap when we have custom generic type should return the most concrete one`` () =
    let result = Types.unwrap typeof<CustomGenericType<CustomGenericType<StringGraphType>>>
    result =! typeof<StringGraphType>

[<Fact>]
let ``unwrap when we have some unwrappable generic type should return the unwrappable type`` () =
    let result = Types.unwrap typeof<CustomGenericType<ListGraphType<StringGraphType>>>
    result =! typeof<ListGraphType<StringGraphType>>

[<Fact>]
let ``unwrap when we have simple type should return it`` () =
    let result = Types.unwrap typeof<StringGraphType>

    result =! typeof<StringGraphType>

[<Fact>]
let ``resolveQueryType when we have custom generic type should return the most concrete one`` () =
    let result = Types.resolveQueryType typeof<CustomGenericType<CustomGenericType<StringGraphType>>>
    result =! typeof<StringGraphType>

[<Fact>]
let ``resolveQueryType when we have simple type should return it`` () =
    let result = Types.resolveQueryType typeof<StringGraphType>

    result =! typeof<StringGraphType>
