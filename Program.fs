open System
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Sentry

// Throws an unhandled exception.
// It will automatically be picked up by Sentry.
let throwHandler : HttpHandler = fun next ctx ->
    // If needed, SentrySdk can also be retrieved from service collection
    // via ctx.RequestServices.GetService<Sentry.IHub>().

    // Add breadcrumbs detailing preceding user actions.
    SentrySdk.AddBreadcrumb ("Opened shopping cart")
    SentrySdk.AddBreadcrumb ("Proceeded to checkout")

    // Add other useful contextual information.
    // (ASP.NET Core related info is added by Sentry automatically)
    SentrySdk.ConfigureScope (fun scope ->
        scope.SetTag ("AB_group", "3") // indexed in search
        scope.SetExtra ("Source", "Giraffe Sample!") // not indexed
        ())

    // Raise an exception, which will be reported and sent to Sentry.
    raise <| Exception ("Expected exception in Giraffe Demo.")

    next(ctx)

let webApp = choose [
    route "/throw" >=> throwHandler
    route "/" >=> text "Go to /throw to raise an exception"
]

let configureServices (services: IServiceCollection) =
    services.AddGiraffe() |> ignore

let configureApp (app: IApplicationBuilder) =
    app.UseGiraffe webApp

[<EntryPoint>]
let main _ =
    WebHost.CreateDefaultBuilder()
        .ConfigureServices(configureServices)
        .Configure(configureApp)
        // Add Sentry integration. With this usage, the DSN and other options are sourced from "appsettings.json".
        // You can also call .UseSentry(fun o -> ...) to configure settings manually.
        .UseSentry()
        .Build()
        .Run()
    0