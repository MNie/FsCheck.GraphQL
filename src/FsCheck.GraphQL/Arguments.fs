module Arguments
    let hasArguments args =
        if args = null then false
        else
            match args |> List.ofSeq with
            | [] -> false
            | _ -> true

    let private format (value: obj) =
        match value with
        | :? string as s -> sprintf "\"%s\"" s
        | :? bool as b -> b.ToString().ToLower()
        | _ -> sprintf "%A" value

    let private toString values =
        match values with
        | [] -> ""
        | _ -> sprintf "%s" (System.String.Join(", ", values))

    let get arg =
        arg.GetType().GetProperties()
        |> Seq.map (fun x -> (x.Name, x.GetValue arg))
        |> Seq.where (fun (_, value) -> value <> null)
        |> Seq.map (fun (name, value) -> sprintf "%s: %s" name (format value))
        |> Seq.toList
        |> toString

