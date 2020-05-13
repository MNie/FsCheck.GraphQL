# FsCheck.GraphQL [![NuGet](https://buildstats.info/nuget/FsCheck.GraphQL?includePreReleases=true)](https://www.nuget.org/packages/FsCheck.GraphQL)

A library which helps to create GraphQL queries for FsCheck tests.

How to use it?
You have to implement a `QueryTestArbitrary` class in your code and in your own implementation invoke `build` or `buildWithArgs` depending on the query which you want to generate.

```fsharp
type CustomQueryArb() =
    inherit QueryTestArbitrary<CustomQuery>()

    member this.create resolver = base.build resolver


type CustomQueryWithArgsArb() =
    inherit QueryTestArbitrary<CustomQueryWithArgs>()

    member this.create resolver fetchArg = this.buildWithArgs resolver fetchArg
```

Where `resolver` is a function to gather an instance of all GraphQL types in your project.
`fetchArgs` whereas is a function to gather all arguments to a query.
