﻿using SharpDox.Model;
using SharpDox.Model.Repository;
using System.Globalization;
using System.IO;
using System.Linq;

namespace SharpDox.Build.Context.Step
{
    internal class ParseProjectStep : StepBase
    {
        private SDProject _sdProject;

        public ParseProjectStep(StepInput stepInput, int progressStart, int progressEnd) :
            base(stepInput, stepInput.SDBuildStrings.StepParseProject, new StepRange(progressStart, progressEnd)) { }

        public override SDProject RunStep(SDProject sdProject)
        {
            _sdProject = sdProject;
            SetProjectInfos();
            GetImages();
            ParseDescriptions();

            if (Path.GetExtension(_stepInput.CoreConfigSection.InputFile) == ".sdnav")
            {
                ParseNavigationFiles();
            }
            else
            {
                _sdProject.Repositories.Add(_stepInput.CoreConfigSection.InputFile, new SDRepository());
            }

            return _sdProject;
        }

        private void SetProjectInfos()
        {
            ExecuteOnStepMessage(_stepInput.SDBuildStrings.ParsingProject);
            ExecuteOnStepProgress(25);

            _sdProject.DocLanguage = _stepInput.CoreConfigSection.DocLanguage;
            _sdProject.LogoPath = _stepInput.CoreConfigSection.LogoPath;
            _sdProject.Author = _stepInput.CoreConfigSection.Author;
            _sdProject.ProjectName = _stepInput.CoreConfigSection.ProjectName;
            _sdProject.VersionNumber = _stepInput.CoreConfigSection.VersionNumber;
            _sdProject.ProjectUrl = _stepInput.CoreConfigSection.ProjectUrl;
            _sdProject.AuthorUrl = _stepInput.CoreConfigSection.AuthorUrl;
        }

        private void GetImages()
        {
            var images = Directory.EnumerateFiles(Path.GetDirectoryName(_stepInput.CoreConfigSection.InputFile), "sdi.*", SearchOption.AllDirectories);
            foreach (var image in images)
            {
                _sdProject.Images.Add(image);
            }
        }

        private void ParseDescriptions()
        {
            ExecuteOnStepMessage(_stepInput.SDBuildStrings.ParsingDescriptions);
            ExecuteOnStepProgress(50);

            var potentialReadMes = Directory.EnumerateFiles(Path.GetDirectoryName(_stepInput.CoreConfigSection.InputFile), "*.sdpd");
            if (potentialReadMes.Any())
            {
                foreach (var readme in potentialReadMes)
                {
                    var splitted = Path.GetFileName(readme).Split('.');
                    if (splitted.Length > 0 && CultureInfo.GetCultures(CultureTypes.NeutralCultures).Any(c => c.TwoLetterISOLanguageName == splitted[0].ToLower()))
                    {
                        if (!_sdProject.Description.ContainsKey(splitted[0].ToLower()))
                        {
                            _sdProject.Description.Add(splitted[0].ToLower(), File.ReadAllText(readme));
                            _sdProject.AddDocumentationLanguage(splitted[0].ToLower());
                        }
                    }
                    else if (splitted.Length > 0 && splitted[0].ToLower() == "default" && !_sdProject.Description.ContainsKey("default"))
                    {
                        _sdProject.Description.Add("default", File.ReadAllText(readme));
                    }
                }
            }
        }

        private void ParseNavigationFiles()
        {
            ExecuteOnStepMessage(_stepInput.SDBuildStrings.ParsingNav);
            ExecuteOnStepProgress(50);

            var navFileParser = new SDNavParser(_stepInput.CoreConfigSection.InputFile);
            var navFiles = Directory.EnumerateFiles(Path.GetDirectoryName(_stepInput.CoreConfigSection.InputFile), "*.sdnav", SearchOption.AllDirectories);
            foreach(var navFile in navFiles)
            {
                _sdProject = navFileParser.ParseNavFile(navFile, _sdProject);
            }
        }
    }
}
