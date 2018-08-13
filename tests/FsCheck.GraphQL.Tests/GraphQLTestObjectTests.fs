module GraphQLTestObjectTests

    open Xunit
    open Swensen.Unquote
    open Models

    let input = { name = "name"; argument = Some "argument: argumentVal"; fields = [ "field1"; "field2" ] }
    let inputWithoutArg = { name = "name"; argument = None; fields = [ "field1"; "field2" ] }

    [<Fact>]
    let ``when calling toString() object should be properly formatted``() =
        let result = input.ToString()

        result =! "{ name (argument: argumentVal) { field1 field2 } }"

    [<Fact>]
    let ``when calling structured format display object should be properly formatted``() =
        let result = sprintf "%A" input

        result =! "{ name (argument: argumentVal) { field1 field2 } }"

    [<Fact>]
    let ``when calling structured format display object for input without args should be properly formatted``() =
        let result = sprintf "%A" inputWithoutArg

        result =! "{ name { field1 field2 } }"
