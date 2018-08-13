module Shrinker
    open Models

    let shrink testObject =
        seq {
            let length = List.length testObject.fields
            if length > 1 then
                let newFields toOmit =
                    testObject.fields
                    |> List.mapi (fun index elem -> if index <> toOmit then Some elem else None)
                    |> List.filter (fun x -> x.IsSome)
                    |> List.map (fun x -> x.Value)

                yield! [0..(length - 1)] |> List.map (fun x -> Some { name = testObject.name; argument = testObject.argument; fields = newFields x })
            else yield None
        }
        |> Seq.filter (fun x -> x.IsSome)
        |> Seq.map (fun x -> x.Value)

