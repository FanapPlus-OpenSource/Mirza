﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Mirza.Web.Data;
using Mirza.Web.Models;
using Mirza.Web.Validators;

namespace Mirza.Web.Services.User
{
    public class UserService : IUserService
    {
        private readonly MirzaDbContext _dbContext;
        private readonly UserValidator _userValidator;
        private readonly ILogger<UserService> _logger;

        public UserService(MirzaDbContext dbContext, ILogger<UserService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
            _userValidator = new UserValidator();
        }
        public async Task<MirzaUser> Register(MirzaUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var validationResult = _userValidator.Validate(user);
            if (!validationResult.IsValid)
            {
                throw new UserModelValidationException(validationResult.Errors.Select(e => e.ErrorMessage).ToArray());
            }

            var duplicateEmail = await _dbContext.UserSet
                .AnyAsync(u => u.IsActive && u.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase))
                .ConfigureAwait(false);

            if (duplicateEmail)
            {
                throw new DuplicateEmailException(user.Email);
            }

            try
            {
                await _dbContext.UserSet.AddAsync(user);
                await _dbContext.SaveChangesAsync().ConfigureAwait(true);
                return user;
            }
            catch (Exception e)
            {
                _logger.LogError("Exception occured while saving user entity", e);
                throw;
            }

        }
    }
}