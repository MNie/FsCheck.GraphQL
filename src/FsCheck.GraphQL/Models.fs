module Models
    
    [<StructuredFormatDisplayAttribute("{format}")>]
    type GrapQLTestObject =
        {
            name: string
            argument: string option
            fields: string list
        }
        member this.format =
            let formatArg =
                match this.argument with
                | None -> System.String.Empty
                | Some s -> sprintf " (%s)" s
            sprintf "{ %s%s { %s } }" this.name formatArg (System.String.Join(' ', this.fields))
        override this.ToString(): string = 
            this.format

