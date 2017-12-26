using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Alexa.NET;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Alexa.NET.Response;
using Amazon.Lambda.Core;
using Newtonsoft.Json;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace RaptorDosing
{
    public class Function
    {
        public static void Log(string logText)
        {
            log.LogLine(logText);
            Console.WriteLine(logText);
        }

        private static ILambdaLogger log = null;

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public SkillResponse FunctionHandler(SkillRequest input, ILambdaContext context)
        {
            SkillResponse response = new SkillResponse
            {
                Response = new ResponseBody
                {
                    ShouldEndSession = false
                }
            };
            log = context.Logger;


            if (input.GetRequestType() == typeof(LaunchRequest))
            {
                Log($"Default LaunchRequest made: 'Alexa, open Medicine Dosage'");
                response = MakeSkillResponse("I'm ready to help you calculate Medicine Dosing. What is the medicine you're dispensing?",
                    false);
            }
            else if (input.GetRequestType() == typeof(IntentRequest))
            {
                var intentRequest = input.Request as IntentRequest;
                switch (intentRequest.Intent.Name)
                {
                    case "AMAZON.CancelIntent":
                        Log($"AMAZON.CancelIntent: send StopMessage");
                        response = MakeSkillResponse("OK, Goodbye.", true);
                        break;
                    case "AMAZON.StopIntent":
                        Log($"AMAZON.StopIntent: send StopMessage");
                        response = MakeSkillResponse("OK, Goodbye.", true);
                        break;
                    case "AMAZON.HelpIntent":
                        Log($"AMAZON.HelpIntent: send HelpMessage");
                        response = MakeSkillResponse("I'll help you figure out how much medicine to administer, based on the animal's weight, the prescribed dose, and the medicine concentration.", false);
                        break;
                    case "GetMedicineDose":
                        Log($"GetMedicineDoseIntent sent: Get the name of the medicine.");
                        response = GetMedicineDose(intentRequest, input, response);
                        break;
                    default:
                        Log($"Unknown intent: " + intentRequest.Intent.Name);
                        response = MakeSkillResponse("You can say Get dosage for a medicine to get started.", false);
                        break;
                }
            }
            Log($"Skill Response Object...");
            Log(JsonConvert.SerializeObject(response));
            return response;
        }

        private SkillResponse GetMedicineDose(IntentRequest intentRequest, SkillRequest input, SkillResponse response)
        {
            switch (intentRequest.DialogState)
            {
                case DialogState.Started:
                    // Pre-fill slots: update the intent object with slot values for which
                    // you have defaults, then return Dialog.Delegate with this updated intent
                    // in the updatedIntent property.
                    Log($"GetMedicineDose: Started");
                    response = ResponseBuilder.DialogDelegate(input.Session, intentRequest.Intent);
                    break;
                case DialogState.InProgress:
                    // return a Dialog.Delegate directive with no updatedIntent property.
                    Log($"GetMedicineDose: InProgress");
                    response = ResponseBuilder.DialogDelegate(input.Session);
                    break;
                case DialogState.Completed:
                    // Dialog is now complete and all required slots should be filled,
                    // so call your normal intent handler. 
                    Log($"GetMedicineDose: Completed");
                    response = CalcDosage(intentRequest, response);
                    break;
                default:
                    // return a Dialog.Delegate directive with no updatedIntent property.
                    //response = ResponseBuilder.DialogElicitSlot(GetInnerResponse("What medicine will you be administering?"), "medicineName", input.Session, intentRequest.Intent);
                    Log($"GetMedicineDose: Default.");
                    Log($"Input: {JsonConvert.SerializeObject(input)}");
                    Log($"Intent Request: {JsonConvert.SerializeObject(intentRequest)}");
                    response = ResponseBuilder.DialogDelegate(input.Session);
                    Log($"Response: {JsonConvert.SerializeObject(response)}");
                    break;
            }
            return response;
        }

        private SkillResponse CalcDosage(IntentRequest intentRequest, SkillResponse response)
        {
            // At this point, we should have all of our parameters, and can perform the calculation.

            string weight = null;
            string medicineName = null;
            string unitOfMeasurement = null;

            //if (intentRequest != null && intentRequest.Intent != null && intentRequest.Intent.Slots != null)
            //{
            //if (intentRequest?.Intent?.Slots.ContainsKey("weight"))
            //{
            weight = intentRequest?.Intent?.Slots["weight"].Value;
            //}
            //if (intentRequest.Intent.Slots.ContainsKey("medicineName"))
            //{
            medicineName = intentRequest?.Intent?.Slots["medicineName"].Value;
            //}
            //if (intentRequest.Intent.Slots.ContainsKey("unitOfMeasurement"))
            //{
            unitOfMeasurement = intentRequest?.Intent?.Slots["unitOfMeasurement"].Value;
            //}
            //}

            if (string.IsNullOrEmpty(weight) || string.IsNullOrEmpty(medicineName) || string.IsNullOrEmpty(unitOfMeasurement))
            {
                response = MakeSkillResponse("Something's wrong. I have a hole where something else should be...", false);
            }
            else
            {
                response = MakeSkillResponse($"I don't have all the data yet to calculate the dosage of {medicineName} for a {weight} {unitOfMeasurement} animal.", false);
                //if (unitOfMeasurement == "Kilograms")
                //{
                //}
            }
            return response;
        }

        private SkillResponse MakeSkillResponse(string outputSpeech, bool shouldEndSession,
                string repromptText = "Just say, get dosage of a medicine. To exit, say, exit.")
        {
            var response = new ResponseBody
            {
                ShouldEndSession = shouldEndSession,
                OutputSpeech = new PlainTextOutputSpeech
                {
                    Text = outputSpeech
                }
            };

            if (repromptText != null)
            {
                response.Reprompt = new Reprompt()
                {
                    OutputSpeech = new PlainTextOutputSpeech()
                    {
                        Text = repromptText
                    }
                };
            }

            var skillResponse = new SkillResponse
            {
                Response = response,
                Version = "1.0"
            };
            return skillResponse;
        }

    }
}

