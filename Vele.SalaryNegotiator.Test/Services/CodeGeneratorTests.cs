using NUnit.Framework;
using Vele.SalaryNegotiator.Core.Services;
using Vele.SalaryNegotiator.Core.Services.Interfaces;

namespace Vele.SalaryNegotiator.Test.Services;

[TestFixture]
public class CodeGeneratorTests
{
    private ICodeGenerator _codeGenerator;

    [SetUp]
    public void Setup()
    {
        _codeGenerator = new WordCodeGenerator();
    }

    [Test]
    public void Generate_ReturnsValidCode()
    {
        string code = _codeGenerator.Generate();

        Assert.NotNull(code);
        Assert.That(code.Length == 12);
    }
}
