module Fields
    open GraphQL.Types
    open System
    open System.Collections.Generic

    type WhatUseToPrint =
        | Field
        | ResolvedType

    let printBasedOnWhatToUse what (field: FieldType) (resolved: IComplexGraphType) =
        match what with
        | WhatUseToPrint.Field -> field.Name
        | WhatUseToPrint.ResolvedType -> resolved.Name

    let rec fieldWithProperties (field: FieldType) (resolver: System.Type -> obj) deep =
        let rec format (resolver: Type -> obj, field: FieldType, deep, toResolve: Type) =
            let formatField (resolverFunc: System.Type -> obj, _, _1, typeToResolve, printFunc, whatUse) =
                let resolvedType = downcast (resolverFunc (typeToResolve)): IComplexGraphType
                let fields = 
                    resolvedType.Fields
                    |> Seq.map (fun x -> fieldWithProperties x resolverFunc (deep + 1))
                    |> fun x -> System.String.Join(" ", x)
                if String.IsNullOrWhiteSpace(fields) then String.Empty
                else printFunc (printBasedOnWhatToUse whatUse field resolvedType) fields

            let formatUnion (resolverFunc: System.Type -> obj, currentField, deepLevel, typeToResolve, printFunc, _) =
                let resolvedType = downcast (resolverFunc (typeToResolve)): UnionGraphType
                resolvedType.Types
                |> Seq.map (fun x -> formatField (resolverFunc, currentField, deepLevel, x, printFunc, WhatUseToPrint.ResolvedType))
                |> fun x -> System.String.Join("", (Printers.printAsUnion resolvedType.Name x))

            let formatInterface (resolverFunc: System.Type -> obj, currentField, deepLevel, typeToResolve, printFunc, _) =
                (downcast (resolverFunc typeof<IEnumerable<IGraphType>>): IEnumerable<IGraphType>)
                |> Seq.where (fun x -> Types.assignableTo<IImplementInterfaces> (x.GetType()))
                |> Seq.map (fun x -> downcast (x): IImplementInterfaces)
                |> Seq.where (fun x -> x.Interfaces |> Seq.contains typeToResolve)
                |> Seq.map (fun x -> formatField(resolverFunc, currentField, deepLevel, x.GetType(), printFunc, WhatUseToPrint.ResolvedType))
                |> fun x -> String.Join("", x)

            let args print whatUse = (resolver, field, deep, toResolve, print, whatUse)
            match toResolve with
            | x when Types.assignableTo<IInterfaceGraphType> x -> 
                (args Printers.printAsSingleUnion WhatUseToPrint.ResolvedType) |> formatInterface 
            | x when Types.assignableTo<IComplexGraphType> x -> 
                (args Printers.printAsObject WhatUseToPrint.Field) |> formatField
            | _ -> (args Printers.printAsSingleUnion WhatUseToPrint.ResolvedType) |> formatUnion
        
        if deep > 3 then System.String.Empty
        else
            let isRegistered = not ((resolver field.Type) = null)
            if isRegistered then format (resolver, field, deep, (Types.unwrap field.Type))
            else field.Name