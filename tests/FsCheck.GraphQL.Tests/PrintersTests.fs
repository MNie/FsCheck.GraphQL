module PrintersTests

    open Xunit
    open Swensen.Unquote

    [<Fact>]
    let ``printAsObject should return properly formatted string``() =
        let result = Printers.printAsObject "SomeName" "field1 field2 field3"
        result =! "SomeName { field1 field2 field3 }"

    [<Fact>]
    let ``printAsSimpleUnion should return properly formatted string``() =
        let result = Printers.printAsSingleUnion "UnionName" "field1 field2 field3"
        result =! "... on UnionName { field1 field2 field3 }"

    [<Fact>]
    let ``printAsUnion should return properly formatted string``() =
        let result = Printers.printAsUnion "SomeName" (seq { yield "... on UnionName { field1 field2 field3 }" })
        result =! "SomeName { ... on UnionName { field1 field2 field3 } }"
