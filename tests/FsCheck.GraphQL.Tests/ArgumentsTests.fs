module ArgumentsTests

    open Xunit
    open Swensen.Unquote
    open System.Collections.Generic

    type ArgsType =
        {
            id: int
            name: string
        }

    type EmptyType() =
        member this.func() = "12"

    type ArgWithAllNullFields(name: string, anotherName: string) =
        member this.name = name
        member this.anotherName = anotherName

    [<Fact>]
    let ``hasArguments when arguments list is empty return false``() =
        let result = Arguments.hasArguments (new List<string>())
        result =! false

    [<Fact>]
    let ``hasArguments when arguments list is null return false``() =
        let result = Arguments.hasArguments null
        result =! false
    
    [<Fact>]
    let ``hasArguments when arguments list is not empty return false``() =
        let input = new List<string>()
        input.Add "singleArg"
        let result = Arguments.hasArguments input
        result =! true

    [<Fact>]
    let ``get when type has some fields return properly formatted fieds as a return value``() =
        let result = Arguments.get {id = 1; name = "test" }

        result =! "id: 1, name: \"test\""

    [<Fact>]
    let ``get when type has some fields return properly formatted fieds as a return value with skipped null values``() =
        let result = Arguments.get {id = 1; name = null }

        result =! "id: 1"

    [<Fact>]
    let ``get when type has not any field should return empty string``() =
        let result = Arguments.get (EmptyType())

        result =! ""

    [<Fact>]
    let ``get when type has all fields set to null should return empty string``() =
        let result = Arguments.get (ArgWithAllNullFields(null, null))

        result =! ""
