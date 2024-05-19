namespace Assignment2ver3

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Templating
open WebSharper.Sitelets

type IndexTemplate = Template<"wwwroot/index.html", ClientLoad.FromDocument>

type EndPoint =
    | [<EndPoint "/">] Calculator
    | [<EndPoint "/portfolios">] Portfolios

[<JavaScript>]
module Pages =
    open WebSharper.UI.Notation
    open WebSharper.JavaScript
    open WebSharper.Charting
    // The templates are loaded from the DOM, so you just can edit index.html
    // and refresh your browser, no need to recompile unless you add or remove holes.

    let Main () =

        let money = Var.Create ""
        //let newName = Var.Create ""
        //let income = JS.Document.GetElementById("inc") :?>HTMLInputElement
        //let incomeval= (income).Value
        //let expenses = "30"
        //let balance =(incomeval - (System.Int32.Parse(expenses)))
        //let b = incomeval.ToString()

        IndexTemplate.Main()

            .OnSend(fun e ->
                let rent = e.Vars.Rent.Value
                let income = e.Vars.Income.Value
                let utilities = e.Vars.Utilities.Value
                let vehicle = e.Vars.Vehicle.Value
                let groceries = e.Vars.Groceries.Value
                let toiletries = e.Vars.Toiletries.Value
                let internet = e.Vars.Internet.Value
                let cable = e.Vars.Cable.Value
                let phone = e.Vars.Phone.Value
                let debt = e.Vars.Debt.Value
                let membership = e.Vars.Membership.Value
                let health = e.Vars.Health.Value
                let travel = e.Vars.Travel.Value
                let other = e.Vars.Other.Value

                let labels =[|"Rent"; "Utilities"; "Vehicle"; "Groceries"; "Toiletries"; "Internet"; "Cable"; "Phone"; "Debt"; "Membership"; "Health"; "Travel"; "Other" |]
                let dataset = [|rent; utilities; vehicle; groceries; toiletries; internet; cable; phone; debt; membership; health; travel; other|]
                    
                let chart =
                        Chart.Radar(Array.zip labels dataset)
                            .WithFillColor(Color.Rgba(151, 187, 205, 0.2))
                            .WithStrokeColor(Color.Name "blue")
                            .WithPointColor(Color.Name "darkblue")
                            .WithTitle("Expenses")
                    
                Renderers.ChartJs.Render(chart, Size = Size(500, 300))

                let balance = 
                    (income - (rent+utilities+vehicle+groceries+toiletries+internet+cable+phone+debt+membership+health+travel+other))
                money := sprintf "%A" balance
            )
            .Balance(money.View)
            .Doc()
    
    let storage = JS.Window.LocalStorage
    let counter =
        let curr = storage.GetItem "counter"
        if curr = "" then
            0
        else
            int curr
        |> Var.Create
    
    let PortfolioPage()=
        IndexTemplate.Stock()
            .Value(View.Map string counter.View)
            .Decrement(fun e ->
                counter := counter.Value - 1
                storage.SetItem("counter", string counter.Value)
            )
            .Increment(fun e ->
                counter := counter.Value + 1
                storage.SetItem("counter", string counter.Value)
            )
            .Doc()
[<JavaScript>]
module App =
    open WebSharper.UI.Notation

    let router = Router.Infer<EndPoint>()
    let currentPage = Router.InstallHash Calculator router

    type Router<'T when 'T: equality> with
        member this.LinkHash (ep: 'T) = "#" + this.Link ep

    [<SPAEntryPoint>]
    let Main () =
        let renderInnerPage (currentPage: Var<EndPoint>) =
            currentPage.View.Map (fun endpoint ->
                match endpoint with
                | Calculator -> Pages.Main()
                | Portfolios   -> Pages.PortfolioPage()
            )
            |> Doc.EmbedView

        IndexTemplate()
            .Url_Home(router.LinkHash EndPoint.Calculator)
            .MainContainer(renderInnerPage currentPage)
            .Bind()
