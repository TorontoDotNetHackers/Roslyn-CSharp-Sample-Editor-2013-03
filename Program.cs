using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Roslyn.Compilers.CSharp;

namespace HelloRoslyn
{
    /// <summary>
    /// A visual syntax editor for C# that highlights code errors using
    /// Roslyn technology. 
    /// <para>
    /// Roslyn has been installed through the NuGet package offering. 
    /// </para>
    /// </summary>
    /// <remarks>
    /// This is a rather scruffy program cobbled together quickly in the timeframe
    /// of a programming session for the purpose
    /// of proving Roslyn's usefulness. For the real
    /// deal see https://compilify.net/
    /// </remarks>
    class CsharpSyntaxEditorProgram
    {
        // Launch the GUI editor. The console window will remain for additional output. 
        static void Main(string[] args)
        {
            LaunchSyntaxEditorForm();
        }

        // A picture of a red "X" to overlay on an error. 
        static PictureBox picX = null;

        /// <summary>
        /// Launches the syntax editor. 
        /// </summary>
        static void LaunchSyntaxEditorForm()
        {
            var initialSource = @" // Edit this code for real-time syntax checking
class Greeter
{
    static void Greet()
    {
        Console.WriteLine(""Hello, World"");
    }
}";
            Form f = new Form
            {
                Text = "C# Syntax Checker using Roslyn",
                Width = 500,
                Height = 300,
            };
            f.MinimumSize = new Size(f.Width, f.Height);

            // Split editor between source view (left side) and marked up syntax view. 
            SplitContainer splitter = new SplitContainer
            {
                Orientation = Orientation.Vertical,
                Top = 0,
                Left = 0,
                Height = f.ClientSize.Height,
                Width = f.ClientSize.Width,
                SplitterWidth = 5,
                Dock = DockStyle.Fill,
                SplitterDistance = f.ClientSize.Width / 2,
            };
            f.Controls.Add(splitter);

            // The source code input text box 
            TextBox sourceCode = new TextBox
            {
                Top = 0,
                Left = 0,
                Multiline = true,
                Width = f.ClientSize.Width,
                Height = f.ClientSize.Height,
                Text = initialSource,
                Dock = DockStyle.Fill,
                WordWrap = false,
            };

            // The rich text syntax highlighter box
            RichTextBox richCode = new RichTextBox
            {
                Top = 0,
                Left = 0,
                Multiline = true,
                Width = f.ClientSize.Width,
                Height = f.ClientSize.Height,
                Text = initialSource,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                WordWrap = false,
                ReadOnly = true,
            };
            // When a key is pressed in the source code box build the syntax view
            sourceCode.KeyUp += (sender, keyArgs) =>
            {
                string scriptText = sourceCode.Text;
                BuildSyntaxView(sourceCode, richCode);
            };

            splitter.Panel1.Controls.Add(sourceCode);
            splitter.Panel2.Controls.Add(richCode);

            // Build the red "X"
            Bitmap bmpMark = new Bitmap(10, 10);
            using (Graphics gfx = Graphics.FromImage(bmpMark))
            {
                Brush b = new SolidBrush(Color.Red);
                Pen p = new Pen(b)
                {
                    Width = 2,
                };
                gfx.DrawLine(p, 0, 0, 10, 10);
                gfx.DrawLine(p, 10, 0, 0, 10);
                gfx.Flush();
            }

            picX = new PictureBox
            {
                Width = 10,
                Height = 10,
                Image = bmpMark,
                Visible = false,
            };

            /* Add the red "X" to the rich text box control because 
             * that is where it will be shown to highlight errors. 
             */
            richCode.Controls.Add(picX);
            picX.BringToFront();

            f.Load += (o, a) =>
            {
                sourceCode.Select(0, 1);
            };

            // Show editor form. 
            Application.Run(f);
        }


        /// <summary>
        ///  Uses Roslyn technology to build a syntax tree from, and acquire
        ///  diagnostics about, the source code. 
        ///  Diagnostics are rendered into the rich text box. If the compiler
        ///  has highlighted a non-empty span then it is highlighted otherwise
        ///  a red X is display at the error pixel point of an empty span.
        /// </summary>
        /// <param name="tbSource">Text box with C# source code.</param>
        /// <param name="richResult">Rich text box to render syntax highlighting into.</param>
        /// <remarks>
        /// Only the first source code syntax error is highlighted. 
        /// </remarks>
        static void BuildSyntaxView(TextBox tbSource, RichTextBox richResult)
        {
            richResult.Text = tbSource.Text;

            // Roslyn!! - SyntaxTree.*
            var syntaxTree = SyntaxTree.ParseText(tbSource.Text);

            var diags = syntaxTree.GetDiagnostics();

            // Display all compiler diagnostics in console 
            Console.WriteLine("============================");
            Console.WriteLine("Compiler has {0} diagnostic messages.", diags.Count());
            // Roslyn again!! 
            foreach (var d in diags)
                Console.WriteLine("> {0}", d.Info.GetMessage());
            Console.WriteLine("-------------------------");

            List<Point> issuePoints = new List<Point>();
            picX.Visible = false;

            foreach (var d in diags)
            {
                // More Roslyn !! - d.*
                if (d.Location.IsInSource)
                {
                    var origFore = Console.ForegroundColor;
                    var origBack = Console.BackgroundColor;

                    Console.BackgroundColor = ConsoleColor.Red;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.Write(tbSource.Text.Substring(d.Location.SourceSpan.Start, d.Location.SourceSpan.Length));

                    // Hey Roslyn !! - d.*
                    int lineEndOffset = d.Location.GetLineSpan(true).StartLinePosition.Line;
                    // Note: line endings in rich text box use one character where textbox input source uses two
                    richResult.Select(d.Location.SourceSpan.Start - lineEndOffset, d.Location.SourceSpan.Length);
                    richResult.SelectionColor = Color.Yellow;
                    richResult.SelectionBackColor = Color.Red;
                    if (d.Location.SourceSpan.Length == 0)
                        issuePoints.Add(richResult.GetPositionFromCharIndex(d.Location.SourceSpan.Start));

                    Console.BackgroundColor = origBack;
                    Console.ForegroundColor = origFore;

                    Console.WriteLine(tbSource.Text.Substring(d.Location.SourceSpan.Start + d.Location.SourceSpan.Length));
                    break; // Stop after first error is highlighted 
                    // No dealing with multiple errors or overlaps in this sample. 
                }
            }

            // Put the X on the first error
            if (issuePoints.Count != 0)
            {
                picX.Location = issuePoints[0];
                picX.Visible = true;
            }

            Console.WriteLine("============================");

        }
    }
}
