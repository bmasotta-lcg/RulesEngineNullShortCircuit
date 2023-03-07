using RulesEngine.Models;
using System.Dynamic;
using System.Linq.Expressions;
using System.Net.WebSockets;
using System.Text.Json;

namespace RulesEngineTests;

internal class Program
{
    static async Task Main(string[] args)
    {
        var engine = CreateRulesEngine();
        var workflow = CreateWorkflow(engine);
        engine.AddWorkflow(workflow);

        var results = new List<RuleResultTree>();
        var textResults = string.Empty;


        //test null property handling in dynamic input
        dynamic dynamicInput = new ExpandoObject();
        dynamicInput.value = null;

        results = await engine.ExecuteAllRulesAsync(workflow.WorkflowName, dynamicInput);
        PrintResults(results, "null property on dynamic input");

        // test null property handling in typed input
        var staticInput = new TypedInput();

        results = await engine.ExecuteAllRulesAsync(workflow.WorkflowName, staticInput);
        PrintResults(results, "null property on typed input");
    }

    private static void PrintResults(List<RuleResultTree> results, string testTitle)
    {
        string textResults = GetJsonResults(results);
        Console.WriteLine(string.Empty);
        Console.WriteLine($"{testTitle.ToUpper()} ".PadRight(100, '*'));
        Console.WriteLine(textResults);
    }

    private static Workflow CreateWorkflow(RulesEngine.RulesEngine engine)
    {
        var workflow = new Workflow()
        {
            WorkflowName = "test",
            Rules = new[] {
                new Rule() {
                    RuleName= "01 - && operator",
                    Expression = "input1.value != null && input1.value == 10"
                },
                new Rule() {
                    RuleName= "02 - AndAlso",
                    Expression = "input1.value != null AndAlso input1.value == 10"
                } ,
                new Rule()
                {
                    RuleName= "03 - null propagation function",
                    Expression = "np(input1.value, 0) == 10"
                },
                new Rule()
                {
                    RuleName= "04 - conditional operator",
                    Expression = "(input1.value == null ? 0 : input1.value) == 10"
                }
            }
        };

        return workflow;
    }

    private static RulesEngine.RulesEngine CreateRulesEngine()
    {
        var settings = new ReSettings();
        return new RulesEngine.RulesEngine(settings);
    }

    private static dynamic GetJsonResults(dynamic result)
    {
        return System.Text.Json.JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
    }

    private class TypedInput
    { 
        public int? Value { get; set; }
    }
}