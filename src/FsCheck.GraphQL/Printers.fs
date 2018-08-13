module Printers

    let printAsObject fieldName fields =
        sprintf "%s { %s }" fieldName fields

    let printAsSingleUnion unionName fields =
        sprintf "... on %s { %s }" unionName fields

    let printAsUnion fieldName (unions: seq<string>) =
        sprintf "%s { %s }" fieldName (System.String.Join(" ", unions))