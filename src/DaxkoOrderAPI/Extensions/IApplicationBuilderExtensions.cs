﻿using DaxkoOrderAPI.Models;
using FluentValidation.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using ValidationException = FluentValidation.ValidationException;

namespace DaxkoOrderAPI.Extensions
{
    public static class IApplicationBuilderExtensions
    {
        public static void UseGlobalExceptionHandling(this IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            app.UseExceptionHandler(appBuilder =>
            {
                appBuilder.Run(async context =>
                {
                    var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();

                    string errorCode = string.Empty;

                    if (exceptionHandlerFeature == null)
                        return;

                    if (exceptionHandlerFeature.Error is ValidationException || exceptionHandlerFeature.Error is System.ComponentModel.DataAnnotations.ValidationException)
                    {
                        var logger = loggerFactory.CreateLogger("Validation logger");
                        logger.LogError(400,
                            exceptionHandlerFeature.Error,
                            exceptionHandlerFeature.Error.Message);

                        string message = string.Empty;
                        if (exceptionHandlerFeature.Error.Message?.StartsWith("Validation failed") == true)
                            message = "A bad request was received.";
                        else
                            message = exceptionHandlerFeature.Error.Message;

                        IEnumerable<ValidationFailure> validationFailures = null;
                        if (exceptionHandlerFeature.Error is ValidationException)
                        {
                            var validationException = (ValidationException)exceptionHandlerFeature.Error;
                            validationFailures = validationException.Errors;
                        }

                        var error = new ErrorResponse
                        {
                            Error = new Error
                            {
                                Message = message,
                                Details = validationFailures?.Select(e => new ErrorDetail
                                {
                                    Message = e.ErrorMessage,
                                    Target = e.PropertyName
                                }).ToArray()
                            }
                        };

                        context.Response.StatusCode = 400;

                        var responseString = Newtonsoft.Json.JsonConvert.SerializeObject(error);

                        await context.Response.WriteAsync(responseString);
                    }
                    else
                    {
                        var logger = loggerFactory.CreateLogger("Global exception logger");
                        logger.LogError(500,
                            exceptionHandlerFeature.Error,
                            exceptionHandlerFeature.Error.Message);

                        errorCode = context.TraceIdentifier;
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsync($"Well that's awkward. Please try again later. Error Code: {errorCode}");
                    }
                });
            });
        }
    }
}
