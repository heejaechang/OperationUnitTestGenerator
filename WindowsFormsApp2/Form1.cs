using System;
using System.Text;
using System.Windows.Forms;
using Microsoft.CodeAnalysis;

namespace WindowsFormsApp2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void OnUnitTestGenerated(object sender, EventArgs e)
        {
            var sb = new StringBuilder();

            var generator = new SourceGenerator();
            var builder = new UnitTestBuilder();

            foreach (var generatedSource in generator.Generates((string)language.SelectedItem == "C#" ? LanguageNames.CSharp : LanguageNames.VisualBasic))
            {
                var unitTest = builder.TryCreate(generatedSource.Name, generatedSource.Language, generatedSource.Source, generatedSource.NodeGetter);
                if (unitTest == null)
                {
                    continue;
                }

                sb.Append(unitTest);
                sb.AppendLine();
            }

            unittestOutput.Text = sb.ToString();
        }
    }
}
