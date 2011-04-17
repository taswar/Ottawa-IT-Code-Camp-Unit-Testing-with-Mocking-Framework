using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OttItCampDemo.Controllers;
using OttItCampDemo.Models;
using Rhino.Mocks;

namespace OttItCampDemo.Tests.Controllers
{
    [TestClass]
    public class AccountControllerMockTests
    {
        [TestMethod]
        public void Register_Get_ReturnsView()
        {
            // Arrange
            var memberService = MockRepository.GenerateStub<IMembershipService>();
            memberService.Stub(x => x.MinPasswordLength).Return(10);

            var controller = new AccountController { MembershipService = memberService };

            ///AccountController controller = GetAccountController();

            // Act
            ActionResult result = controller.Register();

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            Assert.AreEqual(10, ((ViewResult)result).ViewData["PasswordLength"]);
        }


        [TestMethod]
        public void LogOff_LogsOutAndRedirects()
        {
            // Arrange
            var formService = MockRepository.GenerateMock<IFormsAuthenticationService>();
            formService.Expect(x => x.SignOut());
            var controller = new AccountController {FormsService = formService};

            //AccountController controller = GetAccountController();

            // Act
            ActionResult result = controller.LogOff();

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
            RedirectToRouteResult redirectResult = (RedirectToRouteResult)result;
            Assert.AreEqual("Home", redirectResult.RouteValues["controller"]);
            Assert.AreEqual("Index", redirectResult.RouteValues["action"]);
            formService.AssertWasCalled(x => x.SignOut());
            //Assert.IsTrue(((MockFormsAuthenticationService)controller.FormsService).SignOut_WasCalled);
        }

        [TestMethod]
        public void Register_Post_ReturnsViewIfRegistrationFails()
        {
            // Arrange
            var memberService = MockRepository.GenerateStub<IMembershipService>();
            memberService.Stub(x => x.MinPasswordLength).Return(10);
            memberService.Stub(x => x.CreateUser("abc", "def", "ijk")).IgnoreArguments().Return(
                MembershipCreateStatus.DuplicateUserName);
            var controller = new AccountController {MembershipService = memberService};

            //AccountController controller = GetAccountController();
            RegisterModel model = new RegisterModel()
            {
                UserName = "duplicateUser",
                Email = "goodEmail",
                Password = "goodPassword",
                ConfirmPassword = "goodPassword"
            };

            // Act
            ActionResult result = controller.Register(model);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            ViewResult viewResult = (ViewResult)result;
            Assert.AreEqual(model, viewResult.ViewData.Model);
            Assert.AreEqual("Username already exists. Please enter a different user name.", controller.ModelState[""].Errors[0].ErrorMessage);
            Assert.AreEqual(10, viewResult.ViewData["PasswordLength"]);
        }

        [TestMethod]
        public void Register_Post_ReturnsRedirectOnSuccess()
        {
            // Arrange
            var memberService = MockRepository.GenerateStub<IMembershipService>();
            memberService.Stub(x => x.MinPasswordLength).Return(10);
            memberService.Stub(x => x.CreateUser("abc", "def", "ijk")).Return(
                MembershipCreateStatus.Success);

            var formService = MockRepository.GenerateMock<IFormsAuthenticationService>();
            formService.Expect(x => x.SignIn("someUser", false));

            var controller = new AccountController {FormsService = formService, MembershipService = memberService};
            //AccountController controller = GetAccountController();
            RegisterModel model = new RegisterModel()
            {
                UserName = "someUser",
                Email = "goodEmail",
                Password = "goodPassword",
                ConfirmPassword = "goodPassword"
            };

            // Act
            ActionResult result = controller.Register(model);

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
            RedirectToRouteResult redirectResult = (RedirectToRouteResult)result;
            Assert.AreEqual("Home", redirectResult.RouteValues["controller"]);
            Assert.AreEqual("Index", redirectResult.RouteValues["action"]);
            //formService.AssertWasCalled(x => x.SignIn("someUser", false));
            formService.VerifyAllExpectations();
        }

        [TestMethod]
        public void LogOn_Post_ReturnsRedirectOnSuccess_WithLocalReturnUrl()
        {
            var memberService = MockRepository.GenerateStub<IMembershipService>();
            memberService.Stub(x => x.MinPasswordLength).Return(10);
            memberService.Stub(x => x.ValidateUser("someUser", "goodPassword")).Return(true);
            var formService = MockRepository.GenerateMock<IFormsAuthenticationService>();
            formService.Expect(x => x.SignIn("someUser", false));
            // Arrange
           

            var controller = new StubAccountController {FormsService = formService, MembershipService = memberService};

            //AccountController controller = GetAccountController();
            LogOnModel model = new LogOnModel()
            {
                UserName = "someUser",
                Password = "goodPassword",
                RememberMe = false
            };

            // Act
            ActionResult result = controller.LogOn(model, "/someUrl");

            // Assert
            Assert.IsInstanceOfType(result, typeof(RedirectResult));
            RedirectResult redirectResult = (RedirectResult)result;
            Assert.AreEqual("/someUrl", redirectResult.Url);
            formService.VerifyAllExpectations();
        }


    }

    public class StubAccountController: AccountController
    {
        protected override bool IsLocalUrl(string url)
        {
            return true;
        }
    }
}