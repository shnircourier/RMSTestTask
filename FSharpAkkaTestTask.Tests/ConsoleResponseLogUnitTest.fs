module ConsoleResponseLogTests

open System
open NUnit.Framework
open ConsoleResponseLog

[<SetUp>]
let Setup () =
    ()

[<TestFixture>]
type ConsoleResponseLogTests() =

    [<Test>]
    member this.``consoleResponseLog should print the response body to the console``() =
        // Arrange
        let expectedOutput = "Response body:\nTest String\r\n"
        let capturedOutput =
            use sw = new System.IO.StringWriter()
            Console.SetOut(sw)
            consoleResponseLog "Test String"
            sw.ToString()

        // Assert
        Assert.AreEqual(expectedOutput, capturedOutput)
    
    [<Test>]
    member this.``consoleResponseLog should print the 'response body is not string' to the console if pass not string``() =
        // Arrange
        let expectedOutput = "response body is not string\r\n"
        let capturedOutput =
            use sw = new System.IO.StringWriter()
            Console.SetOut(sw)
            consoleResponseLog 1
            sw.ToString()

        // Assert
        Assert.AreEqual(expectedOutput, capturedOutput)