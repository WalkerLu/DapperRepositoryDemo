﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DapperRepository.Core.Domain.Customers;
using DapperRepository.Services.Customers;
using DapperRepository.Web.Models.Customers;

namespace DapperRepository.Web.Controllers
{
    public class CustomerController : Controller
    {
        private readonly ICustomerService _customerService;
        private readonly ICustomerRoleService _customerRoleService;

        public CustomerController(ICustomerService customerService, ICustomerRoleService customerRoleService)
        {
            _customerService = customerService;
            _customerRoleService = customerRoleService;
        }

        public ActionResult Index()
        {
            IEnumerable<CustomerDtoModel> customers = _customerService.GetAllCustomers();
            return View(customers);
        }

        public ActionResult Create()
        {
            var customerRoles = _customerRoleService.GetCustomerRoles().Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString()
            }).ToList();

            CustomerModel model = new CustomerModel
            {
                AvailableRoles = customerRoles
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CustomerModel model)
        {
            if (ModelState.IsValid)
            {
                Customer customer = new Customer
                {
                    Username = model.Username,
                    Email = model.Email,
                    Active = model.Active,
                    CreationTime = DateTime.Now
                };

                _customerService.InsertCustomer(customer, model.RoleId);
            }
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            CustomerDtoModel customer = _customerService.GetCustomerBy(id);
            if (customer == null)
                return RedirectToAction("Index");

            var customerRoles = _customerRoleService.GetCustomerRoles().Select(x => new SelectListItem
            {
                Text = x.Name,
                Value = x.Id.ToString(),
                Selected = x.Id == customer.CustomerRole.Id
            }).ToList();

            CustomerModel model = new CustomerModel
            {
                Id = customer.Id,
                Username = customer.Username,
                Email = customer.Email,
                Active = customer.Active,
                CreationTime = customer.CreationTime,
                RoleId = customer.CustomerRole.Id,
                AvailableRoles = customerRoles
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(CustomerModel model)
        {
            Customer customer = _customerService.GetCustomerById(model.Id);
            if (customer == null)
                return RedirectToAction("Index");

            if (ModelState.IsValid)
            {
                customer.Username = model.Username;
                customer.Email = model.Email;
                customer.Active = model.Active;

                _customerService.UpdateCustomer(customer, model.RoleId);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            Customer customer = _customerService.GetCustomerById(id);
            if (customer == null)
                return Json(new { status = false, msg = "No customer found with the specified id" });

            try
            {
                bool result = _customerService.DeleteCustomer(customer);
                return Json(new { status = result, msg = result ? "deleted successfully" : "deleted failed" });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, msg = ex.Message });
            }
        }
    }
}