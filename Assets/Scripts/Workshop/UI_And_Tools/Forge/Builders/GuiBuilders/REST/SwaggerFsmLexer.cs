// File: Assets/Scripts/Workshop/UI_And_Tools/Forge/Builders/GuiBuilders/REST/SwaggerFsmLexer.cs
using System.Collections.Generic;
using System.Text;

namespace Workshop.UI_And_Tools.Forge.Builders.GuiBuilders.REST
{
    public class SwaggerFsmLexer
    {
        private enum ParseState
        {
            SeekingPaths,
            SeekingRoute,
            SeekingMethod,
            SeekingSummary,
            Done
        }

        public RestApiDefinition Parse(string rawJson)
        {
            var apiDef = new RestApiDefinition { SystemName = "Automated Ingestion" };

            ParseState state = ParseState.SeekingPaths;
            int depth = 0;
            int pathsDepth = -1;

            string currentRoute = "";
            string currentMethod = "";

            // Fast forward index
            for (int i = 0; i < rawJson.Length; i++)
            {
                char c = rawJson[i];

                // Track structural depth
                if (c == '{') depth++;
                else if (c == '}') depth--;

                switch (state)
                {
                    case ParseState.SeekingPaths:
                        if (MatchKeyword(rawJson, i, "\"paths\""))
                        {
                            pathsDepth = depth; // We are entering the paths object
                            state = ParseState.SeekingRoute;
                        }
                        break;

                    case ParseState.SeekingRoute:
                        // If we drop below the paths depth, we are done with all endpoints
                        if (depth < pathsDepth)
                        {
                            state = ParseState.Done;
                            break;
                        }

                        // Routes are keys at pathsDepth + 1 that start with "/"
                        if (depth == pathsDepth && c == '"' && rawJson[i + 1] == '/')
                        {
                            currentRoute = ExtractString(rawJson, i + 1);
                            state = ParseState.SeekingMethod;
                        }
                        break;

                    case ParseState.SeekingMethod:
                        // If we drop below route depth, go back to seeking the next route
                        if (depth <= pathsDepth)
                        {
                            state = ParseState.SeekingRoute;
                            i--; // Re-evaluate character in new state
                            break;
                        }

                        // Methods are keys at pathsDepth + 1
                        if (depth == pathsDepth + 1 && c == '"')
                        {
                            string potentialMethod = ExtractString(rawJson, i + 1).ToUpper();
                            if (IsHttpMethod(potentialMethod))
                            {
                                currentMethod = potentialMethod;
                                state = ParseState.SeekingSummary;
                            }
                        }
                        break;

                    case ParseState.SeekingSummary:
                        // If we drop below method depth, save the endpoint and go find the next method
                        if (depth <= pathsDepth + 1)
                        {
                            // We finished looking at this method, save it even if we didn't find a summary
                            apiDef.Endpoints.Add(new RestEndpoint
                            {
                                Route = currentRoute,
                                Method = currentMethod,
                                Name = $"{currentMethod} {currentRoute}" // Fallback name
                            });

                            state = ParseState.SeekingMethod;
                            i--; // Re-evaluate character
                            break;
                        }

                        // Look for the summary key
                        if (MatchKeyword(rawJson, i, "\"summary\""))
                        {
                            // Skip the colon and whitespace to get the value
                            int valueStart = FindNextQuote(rawJson, i + 9);
                            string summary = ExtractString(rawJson, valueStart + 1);

                            apiDef.Endpoints.Add(new RestEndpoint
                            {
                                Route = currentRoute,
                                Method = currentMethod,
                                Name = summary // Use the actual summary from the API
                            });

                            // Fast forward to end of this method's object to avoid duplicate parsing
                            state = ParseState.SeekingMethod;
                        }
                        break;

                    case ParseState.Done:
                        return apiDef;
                }
            }

            return apiDef;
        }

        // --- Helper Methods for the Lexer ---

        private bool MatchKeyword(string text, int index, string keyword)
        {
            if (index + keyword.Length > text.Length) return false;
            for (int i = 0; i < keyword.Length; i++)
            {
                if (text[index + i] != keyword[i]) return false;
            }
            return true;
        }

        private string ExtractString(string text, int startIndex)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = startIndex; i < text.Length; i++)
            {
                if (text[i] == '"' && text[i - 1] != '\\') break; // Stop at unescaped quote
                sb.Append(text[i]);
            }
            return sb.ToString();
        }

        private int FindNextQuote(string text, int startIndex)
        {
            for (int i = startIndex; i < text.Length; i++)
            {
                if (text[i] == '"') return i;
            }
            return startIndex;
        }

        private bool IsHttpMethod(string method)
        {
            return method == "GET" || method == "POST" || method == "PUT" || method == "DELETE" || method == "PATCH";
        }
    }
}