using FakeXrmEasy;
using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace DFE.GITIS.CRM.Workflows.Tests
{
    public class CheckForRecentSMSMessageWorkflowTests
    {
        private XrmFakedContext _context;
        private string _noValidNumberOutput = "NoValidNumber";
        private string _duplicateCheckHours = "5";


        public CheckForRecentSMSMessageWorkflowTests()
        {
            _context = new XrmFakedContext
            {
                ProxyTypesAssembly = Assembly.Load("DFE.GITIS.CRM.ProxyClasses")
            };
        }

        [Fact]
        public void CandidateWithNoNumbersReturnsNoValidNumber()
        {
            var inputs = new Dictionary<string, object>() {
                { "_homeNumber", null },
                { "_workNumber", null },
                { "_mobileNumber", null },
                { "_duplicateCheckHours", _duplicateCheckHours }
            };

            var result = _context.ExecuteCodeActivity<CheckForRecentSMSMessage>(inputs);

            Assert.True((string)result["_smsNumberToContact"] == _noValidNumberOutput);
        }

        [Fact]
        public void CandidateWithNoValidPhoneNumberOutputsAsInvalidNumber()
        {
            var inputs = new Dictionary<string, object>() {
                { "_homeNumber", null },
                { "_workNumber", null },
                { "_mobileNumber", null },
                { "_duplicateCheckHours", _duplicateCheckHours }
            };

            var result = _context.ExecuteCodeActivity<CheckForRecentSMSMessage>(inputs);

            Assert.True((string)result["_smsNumberToContact"] == _noValidNumberOutput);  
        }

        [Fact]
        public void CandidateWithOnlyValidHomePhoneNumberOutputsAsValidNumber()
        {
            var homeNumber = "07983716208";

            var inputs = new Dictionary<string, object>() {
                { "_homeNumber", homeNumber },
                { "_workNumber", null },
                { "_mobileNumber", null },
                { "_duplicateCheckHours", _duplicateCheckHours }
            };

            var result = _context.ExecuteCodeActivity<CheckForRecentSMSMessage>(inputs);

            Assert.True((string)result["_smsNumberToContact"] == homeNumber);
        }

        [Fact]
        public void CandidateWithOnlyValidWorkPhoneNumberOutputsAsValidNumber()
        {
            var workNumber = "07983716208";

            var inputs = new Dictionary<string, object>() {
                { "_homeNumber", null },
                { "_workNumber", workNumber },
                { "_mobileNumber", null },
                { "_duplicateCheckHours", _duplicateCheckHours }
            };

            var result = _context.ExecuteCodeActivity<CheckForRecentSMSMessage>(inputs);

            Assert.True((string)result["_smsNumberToContact"] == workNumber);

        }

        [Fact]
        public void CandidateWithOnlyValidMobilePhoneNumberOutputsAsValidNumber()
        {
            var mobileNumber = "07983716208";

            var inputs = new Dictionary<string, object>() {
                { "_homeNumber", null },
                { "_workNumber", mobileNumber },
                { "_mobileNumber", null },
                { "_duplicateCheckHours", _duplicateCheckHours }
            };

            var result = _context.ExecuteCodeActivity<CheckForRecentSMSMessage>(inputs);

            Assert.True((string)result["_smsNumberToContact"] == mobileNumber);

        }

        [Fact]
        public void CandidateWithOnlyValidHomePhoneNumberWith44SyntaxOutputsAsFormattedNumber()
        {
            var homeNumberPreFormat = "447983716208";
            var homeNumberPostFormat = "07983716208";

            var inputs = new Dictionary<string, object>() {
                { "_homeNumber", homeNumberPreFormat },
                { "_workNumber", null },
                { "_mobileNumber", null },
                { "_duplicateCheckHours", _duplicateCheckHours }
            };

            var result = _context.ExecuteCodeActivity<CheckForRecentSMSMessage>(inputs);

            Assert.True((string)result["_smsNumberToContact"] == homeNumberPostFormat);
        }

        [Fact]
        public void CandidateWithOnlyValidWorkPhoneNumberWith44SyntaxOutputsAsFormattedNumber()
        {
            var workNumberPreFormat = "447983716208";
            var workNumberPostFormat = "07983716208";

            var inputs = new Dictionary<string, object>() {
                { "_homeNumber", null },
                { "_workNumber", workNumberPreFormat },
                { "_mobileNumber", null },
                { "_duplicateCheckHours", _duplicateCheckHours }
            };

            var result = _context.ExecuteCodeActivity<CheckForRecentSMSMessage>(inputs);

            Assert.True((string)result["_smsNumberToContact"] == workNumberPostFormat);
        }

        [Fact]
        public void CandidateWithOnlyValidMobilePhoneNumberWith44SyntaxOutputsAsFormattedNumber()
        {
            var mobileNumberPreFormat = "447983716208";
            var mobileNumberPostFormat = "07983716208";

            var inputs = new Dictionary<string, object>() {
                { "_homeNumber", null },
                { "_workNumber", null },
                { "_mobileNumber", mobileNumberPreFormat },
                { "_duplicateCheckHours", _duplicateCheckHours }
            };

            var result = _context.ExecuteCodeActivity<CheckForRecentSMSMessage>(inputs);

            Assert.True((string)result["_smsNumberToContact"] == mobileNumberPostFormat);
        }

        [Fact]
        public void CandidateThatHasAll3NumbersPopulatedHomeIsChosenFirst()
        {
            var homeNumberPreFormat = "07983716207";
            var workNumberPreFormat = "07983716208";
            var mobileNumberPreFormat = "07983716209";

            var inputs = new Dictionary<string, object>() {
                { "_homeNumber", homeNumberPreFormat },
                { "_workNumber", workNumberPreFormat },
                { "_mobileNumber", mobileNumberPreFormat },
                { "_duplicateCheckHours", _duplicateCheckHours }
            };

            var result = _context.ExecuteCodeActivity<CheckForRecentSMSMessage>(inputs);

            Assert.True((string)result["_smsNumberToContact"] == homeNumberPreFormat);
        }

        [Fact]
        public void CandidateThatHasValidMobileAndInvalidHomeMobileIsChosen()
        {
            var homeNumberPreFormat = "load of rubbish";
            var workNumberPreFormat = "07983716208";
            var mobileNumberPreFormat = "07983716209";

            var inputs = new Dictionary<string, object>() {
                { "_homeNumber", homeNumberPreFormat },
                { "_workNumber", workNumberPreFormat },
                { "_mobileNumber", mobileNumberPreFormat },
                { "_duplicateCheckHours", _duplicateCheckHours }
            };

            var result = _context.ExecuteCodeActivity<CheckForRecentSMSMessage>(inputs);

            Assert.True((string)result["_smsNumberToContact"] == mobileNumberPreFormat);
        }

        [Fact]
        public void CandidateThatHasInvalidHomeAndMobileButValidWorkNumberWorkIsChosen()
        {
            var homeNumberPreFormat = "load of rubbish";
            var workNumberPreFormat = "07983716208";
            var mobileNumberPreFormat = "000007983716209";

            var inputs = new Dictionary<string, object>() {
                { "_homeNumber", homeNumberPreFormat },
                { "_workNumber", workNumberPreFormat },
                { "_mobileNumber", mobileNumberPreFormat },
                { "_duplicateCheckHours", _duplicateCheckHours }
            };

            var result = _context.ExecuteCodeActivity<CheckForRecentSMSMessage>(inputs);

            Assert.True((string)result["_smsNumberToContact"] == workNumberPreFormat);
        }

        [Fact]
        public void CandidateThatHasOnlyMobileAndNeedsFormattingIsChosenAndOutputCorrectly()
        {
            var workNumberPreFormat = "0447983716208";
            var workNumberPostFormat = "07983716208";

            var inputs = new Dictionary<string, object>() {
                { "_homeNumber", null },
                { "_workNumber", workNumberPreFormat },
                { "_mobileNumber", null },
                { "_duplicateCheckHours", _duplicateCheckHours }
            };

            var result = _context.ExecuteCodeActivity<CheckForRecentSMSMessage>(inputs);

            Assert.True((string)result["_smsNumberToContact"] == workNumberPostFormat);
        }

        [Fact]
        public void CandidateThatHasNoneDigitCharactersInTheirNumbersAreDiscounted()
        {
            var workNumberPreFormat = "07983g16208";

            var inputs = new Dictionary<string, object>() {
                { "_homeNumber", null },
                { "_workNumber", workNumberPreFormat },
                { "_mobileNumber", null },
                { "_duplicateCheckHours", _duplicateCheckHours }
            };

            var result = _context.ExecuteCodeActivity<CheckForRecentSMSMessage>(inputs);

            Assert.True((string)result["_smsNumberToContact"] == _noValidNumberOutput);
        }

        [Fact]
        public void CandidateThatHasPlusNumberIsFormattedCorrectly()
        {
            var workNumberPreFormat = "+447983716207";
            var workNumberPostFormat = "07983716207";

            var inputs = new Dictionary<string, object>() {
                { "_homeNumber", null },
                { "_workNumber", workNumberPreFormat },
                { "_mobileNumber", null },
                { "_duplicateCheckHours", _duplicateCheckHours }
            };

            var result = _context.ExecuteCodeActivity<CheckForRecentSMSMessage>(inputs);

            Assert.True((string)result["_smsNumberToContact"] == workNumberPostFormat);
        }

        [Fact]
        public void CandidateThat0044NumberIsFormattedCorrectly()
        {
            var workNumberPreFormat = "00447983716207";
            var workNumberPostFormat = "07983716207";

            var inputs = new Dictionary<string, object>() {
                { "_homeNumber", null },
                { "_workNumber", workNumberPreFormat },
                { "_mobileNumber", null },
                { "_duplicateCheckHours", _duplicateCheckHours }
            };

            var result = _context.ExecuteCodeActivity<CheckForRecentSMSMessage>(inputs);

            Assert.True((string)result["_smsNumberToContact"] == workNumberPostFormat);
        }

    }
}
