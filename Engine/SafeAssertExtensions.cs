using System;
using NUnit.Framework;
using Assert = CSharpSeleniumFramework.Engine.AssertWithScreenshot;

namespace CSharpSeleniumFramework.Engine
{
    public static class SafeAssert
    {
        public static void AreEqual(string verwachteTekst, string teVergelijken,
            string foutmeldingVerwachtResultaat = "The validation was not successful")
        {
            try
            {
                if (verwachteTekst == null) throw new NullReferenceException("The expected field is empty.");
                Assert.AreEqual(verwachteTekst, teVergelijken, foutmeldingVerwachtResultaat + " (Please note, the screenshot for this test was not taken at the time of this message)");
            }
            catch (AssertionException)
            {
            }
        }

        public static void IsTrue(bool statement, string errorMessageExpectedResult = "The validation was not successful")
        {
            try
            {
                Assert.IsTrue(statement, errorMessageExpectedResult + " (Please note, the screenshot for this test was not taken at the time of this message)");
            }
            catch (AssertionException)
            {
            }
        }

        public static void IsFalse(bool statement,
            string foutmeldingVerwachtResultaat = "The validation was not successful")
        {
            try
            {
                Assert.IsFalse(statement, foutmeldingVerwachtResultaat + " (Please note, the screenshot for this test was not taken at the time of this message)");
            }
            catch (AssertionException)
            {
            }
        }

        public static void Fail(string foutmeldingVerwachtResultaat = "The validation was not successful")
        {
            try
            {
                Assert.Fail(foutmeldingVerwachtResultaat + " (Please note, the screenshot for this test was not taken at the time of this message)");
            }
            catch (AssertionException)
            {
            }
        }
    }
}