using System;
using System.Threading.Tasks;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.ClientModel;
using Microsoft.Extensions.Configuration;  // <-- Required for ConfigurationBuilder

class Program
{
    static async Task Main()
    {
        // ✅ Step 1: Load configuration from appsettings.json
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var endpoint = new Uri(config["AzureOpenAI:Endpoint"]);
        var apiKey = config["AzureOpenAI:ApiKey"];
        var deploymentName = config["AzureOpenAI:DeploymentName"];

        // ✅ Step 2: Create an AzureOpenAIClient
        var client = new AzureOpenAIClient(endpoint, new ApiKeyCredential(apiKey))
            .GetChatClient(deploymentName)
            .AsIChatClient();  // Convert to IChatClient

        // ✅ Step 3: Define a helper to create translation agents
        static AIAgent GetTranslationAgent(string targetLanguage, IChatClient chatClient) =>
            chatClient.CreateAIAgent(
                instructions: $"You are a translator that only replies in {targetLanguage}. " +
                              $"Detect the input language and translate it into {targetLanguage}.",
                name: $"Translator_{targetLanguage}"
            );

        // ✅ Step 4: Create agents for multiple languages
        var translationAgents = new[]
        {
            GetTranslationAgent("French", client),
            GetTranslationAgent("Spanish", client),
            GetTranslationAgent("English", client)
        };

        // ✅ Step 5: Sequentially run the workflow
        Console.WriteLine("🌐 Sequential Translation Workflow Started...\n");
        Console.Write("Enter text to translate: ");
        string input = Console.ReadLine();

        var currentText = input;

        foreach (var agent in translationAgents)
        {
            Console.WriteLine($"\n--- {agent.Name} ---");
            var result = await agent.RunAsync(currentText);
            currentText = result.ToString();
            Console.WriteLine(currentText);
        }

        Console.WriteLine("\n✅ Final Sequential Translation Output:\n");
        Console.WriteLine(currentText);
    }
}
