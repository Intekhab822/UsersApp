﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UsersApp.Models;
using UsersApp.ViewModels;

namespace UsersApp.Controllers
{
    public class AccountController : Controller
    {

        private readonly SignInManager<Users> signInManager;
        private readonly UserManager<Users> userManager;
        private object modelstate;

        public AccountController(SignInManager<Users> signInManager, UserManager<Users> userManager)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(ViewModels.LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError(string.Empty, "Email or Password is incorrect");
                return View(model);
            }
            return View(model);
        }

        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(ViewModels.VerifyViewModel model)
        {
            if (ModelState.IsValid)
            {
                Users users = new()
                {
                    FullName = model.Name,
                    Email = model.Email,
                    UserName = model.Email
                };

                var result = await userManager.CreateAsync(users, model.Password);
                if (result.Succeeded)
                {
                    await signInManager.SignInAsync(users, isPersistent: false);
                    return RedirectToAction("Login", "Account");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult VerifyEmail()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> VerifyEmail(VerifyEmailViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByNameAsync(model.Email);
                {
                    if (user == null)
                    {
                        ModelState.AddModelError("", "somtheing went wrong");
                        return View(model);
                    }
                    else
                    {
                        return RedirectToAction("ChangePassword", "Account", new { username = user.UserName });
                    }

                }

            }
            return View(model);
        }


        public IActionResult ChangePassword(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("VerifyEmail", "Account");
            }
            return View(new ChangePasswordViewModel { Email = username });
        }

        [HttpPost]

        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByNameAsync(model.Email);
                {
                    if (user == null)
                    {
                        var result = await userManager.RemovePasswordAsync(user);
                        if (result.Succeeded)
                        {
                            result = await userManager.AddPasswordAsync(user, model.NewPassword);
                            return RedirectToAction("Login", "Account");
                        }

                        else
                        {
                            foreach (var error in result.Errors)
                            {
                                ModelState.AddModelError("", error.Description);
                            }
                        }
                        return View(model);
                    }
                    else
                    {
                        ModelState.AddModelError("", "Email not Found! ");
                        return View(model);
                    }


                }


            }

            else
            {
                ModelState.AddModelError("", "Something went wrong. try again ");
                return View(model);
            }
        }


        public async Task<IActionResult>Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

    }

}


