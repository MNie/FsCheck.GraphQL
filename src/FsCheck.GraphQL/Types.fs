module Types
    open System
    open GraphQL.Types
    open System.Reflection

    let assignableTo<'a> (t: Type) =
        typeof<'a>.GetTypeInfo().IsAssignableFrom(t.GetTypeInfo());

    let rec unwrap (fieldType: Type): Type =
        let unwrappableTypes = [
            typedefof<ListGraphType<'a>>
            typedefof<NonNullGraphType<'a>>
            typedefof<EnumerationGraphType<'a>>
        ]
        let shouldntBeUnwraped t = unwrappableTypes |> List.contains (fieldType.GetGenericTypeDefinition())
        match fieldType with
        | x when not fieldType.IsGenericType -> x
        | x when (shouldntBeUnwraped fieldType) -> x
        | _ -> unwrap (fieldType.GetGenericArguments() |> Array.last)
        
    let rec resolveQueryType (queryType: Type): Type =
        let ``type`` = 
            if queryType.IsGenericType then queryType.GetGenericArguments() |> Array.tryHead |> function | Some x -> x | None -> queryType 
            else queryType
        match ``type``.IsGenericType with
        | true -> resolveQueryType ``type``
        | false -> ``type``