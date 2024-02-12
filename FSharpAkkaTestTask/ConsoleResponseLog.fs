module ConsoleResponseLog

let consoleResponseLog (msg: obj) =
        match msg with
        | :? string as responseBody -> printfn $"Response body:\n{responseBody}"
        | _ -> printfn "response body is not string"