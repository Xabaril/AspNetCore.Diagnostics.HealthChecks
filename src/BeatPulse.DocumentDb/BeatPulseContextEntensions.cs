using BeatPulse.Core;
using BeatPulse.DocumentDb;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace BeatPulse
{
    public static class BeatPulseContextEntensions
    {
        public static BeatPulseContext AddDocumentDb(this BeatPulseContext context, Action<DocumentDbOptions> options, string name = nameof(DocumentDbLiveness), string defaultPath = "documentdb")
        {
            var documentDbOptions = new DocumentDbOptions();
            options(documentDbOptions);

            context.AddLiveness(name, setup =>
            {
                setup.UsePath(defaultPath);
                setup.UseFactory(sp => new DocumentDbLiveness(documentDbOptions, sp.GetService<ILogger<DocumentDbLiveness>>()));
            });

            return context;
        }
    }
}
