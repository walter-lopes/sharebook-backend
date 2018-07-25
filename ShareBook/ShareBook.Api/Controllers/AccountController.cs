﻿using AutoMapper;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using ShareBook.Api.ViewModels;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Repository.Infra.CrossCutting.Identity;
using ShareBook.Repository.Infra.CrossCutting.Identity.Configurations;
using ShareBook.Repository.Infra.CrossCutting.Identity.Interfaces;
using ShareBook.Service;
using System;

namespace ShareBook.Api.Controllers
{
    [Route("api/[controller]")]
    [EnableCors("AllowAllHeaders")]
    public class AccountController : Controller
    {
        private readonly IUserService _userService;
        private readonly IApplicationSignInManager _signManager;

        public AccountController(IUserService userService, IApplicationSignInManager signManager)
        {
            _userService = userService;
            _signManager = signManager;
        }

        [HttpPost("Register")]
        public object Post([FromBody]RegisterUserVM registerUserVM, 
            [FromServices]SigningConfigurations signingConfigurations,
            [FromServices]TokenConfigurations tokenConfigurations)
        {
            var user = Mapper.Map<RegisterUserVM, User>(registerUserVM);
           
            var result = _userService.Insert(user);

            if (result.Success)
                return _signManager.GenerateTokenAndSetIdentity(result.Value, signingConfigurations, tokenConfigurations);

            return result;
        }

        [HttpPost("Login")]
        public object Login([FromBody]LoginUserVM loginUserVM,
            [FromServices]SigningConfigurations signingConfigurations,
            [FromServices]TokenConfigurations tokenConfigurations)
        {

            var user = Mapper.Map<LoginUserVM, User>(loginUserVM);

            var result = _userService.AuthenticationByEmailAndPassword(user);

            if (result.Success)
                return _signManager.GenerateTokenAndSetIdentity(result.Value, signingConfigurations, tokenConfigurations);

            return result;
        }

        [HttpGet("UserById/{id}")]
        public User UserById(string id)
        {
           return _userService.Get(new Guid(id));
        }

        [HttpPut("Update/{id}")]
        public object Update(string id, [FromBody]UpdateUserVM updateUserVM,
           [FromServices]SigningConfigurations signingConfigurations,
           [FromServices]TokenConfigurations tokenConfigurations)
        {
            var user = Mapper.Map<UpdateUserVM, User>(updateUserVM);
            user.Id = new Guid(id);

            var result = _userService.Update(user);

            if (result.Success)
                return _signManager.GenerateTokenAndSetIdentity(result.Value, signingConfigurations, tokenConfigurations);

            return result;
        }

        [HttpPut("ChangePassword")]
        public Result<User> ChangePassword([FromBody]ChangePasswordUserVM changePasswordUserVM)
        {
            var user = new User(){
                Email = changePasswordUserVM.Email,
                Password = changePasswordUserVM.NewPassword
            };

           return  _userService.ChangeUserPassword(user, changePasswordUserVM.OldPassword);
        }
    }
}
