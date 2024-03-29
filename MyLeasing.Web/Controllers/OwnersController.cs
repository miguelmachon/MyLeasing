﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyLeasing.Web.Data;
using MyLeasing.Web.Data.Entities;
using MyLeasing.Web.Helpers;
using MyLeasing.Web.Models;

namespace MyLeasing.Web.Controllers
{

    [Authorize(Roles = "Manager")]
    public class OwnersController : Controller
    {
        private readonly DataContext _datacontext;
        private readonly IUserHelper _userHelper;
        private readonly ICombosHelper _combosHelper;
        private readonly IConverterHelper _converterHelper;

        public OwnersController(
            DataContext datacontext,
            IUserHelper userHelper,
            ICombosHelper combosHelper ,
            IConverterHelper converterHelper 
            )
        {
            _datacontext = datacontext;
            _userHelper = userHelper;
            _combosHelper = combosHelper;
            _converterHelper = converterHelper;
        }

        // GET: Owners
        public IActionResult Index()
        {
            return View( _datacontext.Owners
                .Include(o => o.User)
                .Include(o => o.Properties)
                .Include(o => o.Contracts ));
        }

        // GET: Owners/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var owner = await _datacontext.Owners.Include(o => o.User)
                .Include(o => o.User)
                .Include(o => o.Contracts)
                .ThenInclude(c => c.Lessee)
                .ThenInclude(l => l.User)
                .Include(o => o.Properties)
                .ThenInclude(p => p.PropertyImages)
                .FirstOrDefaultAsync(m => m.Id == id);



            if (owner == null)
            {
                return NotFound();
            }

            return View(owner);
        }

        // GET: Owners/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Owners/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await CreateUserAsync(model);

                if (user != null )
                {
                    var owner = new Owner
                    {
                        Contracts = new List<Contract>(),
                        Properties = new List<Property>(),
                        User = user 

                    };

                    _datacontext.Owners.Add(owner);
                    await _datacontext.SaveChangesAsync();
                    return RedirectToAction("Index");

                };

                ModelState.AddModelError(string.Empty, "User whit this email already exist");


             }
            return View(model);
        }

        private async Task<User> CreateUserAsync(AddUserViewModel model)
        {
            var user = new User
            {
                Address = model.Address,
                Document = model.Document,
                Email = model.Username ,
                 FirstName = model.FirstName ,
                 LastName = model.LastName ,
                 PhoneNumber = model.PhoneNumber,
                 UserName = model.Username

            };

            var result = await _userHelper.AddUserAsync(user, model.Password);

            if (result.Succeeded)
            {
                user = await _userHelper.GetUserByEmailAsync(model.Username);

                await _userHelper.AddUserToRoleAsync(user, "Owner");

                return user;

            }

            return null;

        }

        // GET: Owners/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var owner = await _datacontext.Owners.FindAsync(id);
            if (owner == null)
            {
                return NotFound();
            }
            return View(owner);
        }

        // POST: Owners/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id")] Owner owner)
        {
            if (id != owner.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _datacontext.Update(owner);
                    await _datacontext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OwnerExists(owner.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(owner);
        }

        // GET: Owners/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var owner = await _datacontext.Owners
                .FirstOrDefaultAsync(m => m.Id == id);
            if (owner == null)
            {
                return NotFound();
            }

            return View(owner);
        }

        // POST: Owners/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var owner = await _datacontext.Owners.FindAsync(id);
            _datacontext.Owners.Remove(owner);
            await _datacontext.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OwnerExists(int id)
        {
            return _datacontext.Owners.Any(e => e.Id == id);
        }

        [HttpGet]
        public async Task<IActionResult> AddProperty(int? id)
        {
            if (id == null)
            {
                return NotFound();
            };

            var owner = await _datacontext.Owners.FindAsync(id.Value);

            if (owner == null)
            {
                return NotFound();
            }

            var model = new PropertyViewModel
            {
                OwnerId = owner.Id ,
                PropertyTypes =_combosHelper.GetComboProperyType()
            };

            return View(model);

        }

        [HttpPost]
        public async Task<IActionResult> AddProperty(PropertyViewModel model)
        {
            if (ModelState.IsValid)
            {
                var property = await _converterHelper.ToPropertyAsync(model, true);

                _datacontext.Properties.Add(property);

                await _datacontext.SaveChangesAsync();

                return RedirectToAction($"Details/{model.Id}");

            }
            
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditProperty(int? id)
        {
            if (id == null)
            {
                return NotFound();
            };

            var property = await _datacontext.Properties
                .Include(p => p.Owner)
                .Include(p => p.PropertyType)
                .FirstOrDefaultAsync( p => p.Id == id.Value);

            if (property == null)
            {
                return NotFound();
            }

            var model = _converterHelper.ToPropertyViewModelAsync(property);




            return View(model);

        }

        [HttpPost]
        public async Task<IActionResult> EditProperty(PropertyViewModel model)
        {

            if (ModelState.IsValid)
            {

                var property = await _converterHelper.ToPropertyAsync(model, false);

                _datacontext.Properties.Update(property);

                await _datacontext.SaveChangesAsync();

                return RedirectToAction($"Details/{model.OwnerId}");


            };




            return View(model);

        }


    }
}
