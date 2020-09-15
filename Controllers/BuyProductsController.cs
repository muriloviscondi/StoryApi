﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using APIStory.Context;
using APIStory.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;

namespace APIStory.Controllers
{
  [Authorize(AuthenticationSchemes = "Bearer")]
  [Route("[Controller]")]
  [ApiController]
  [EnableCors("EnableAll")]
  public class BuyProductsController : ControllerBase
  {
    private readonly AppDbContext _context;

    public BuyProductsController(AppDbContext context)
    {
      _context = context;
    }

    // GET: BuyProducts
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BuyProduct>>> GetBuyProducts()
    {
      return await _context.BuyProducts
        .Include(x => x.Product)
        .Include(x => x.User)
        .ToListAsync();
    }

    // GET: BuyProducts/5
    [HttpGet("{id}")]
    public async Task<ActionResult<BuyProduct>> GetBuyProduct(int id)
    {
      var buyProduct = await _context.BuyProducts.FindAsync(id);
      
      if (buyProduct == null)
      {
        return NotFound();
      }

      return buyProduct;
    }

    // PUT: BuyProducts/5
    [HttpPut("update/{id}")]
    public async Task<IActionResult> PutBuyProduct(int id, BuyProduct buyProduct)
    {
      var data = _context.BuyProducts.FindAsync(id);

      if (id != buyProduct.BuyProductId)
      {
        return BadRequest();
      }

      _context.Entry(buyProduct).State = EntityState.Modified;

      try
      {
        await _context.SaveChangesAsync();
      }
      catch (DbUpdateConcurrencyException)
      {
        if (!BuyProductExists(id))
        {
          return NotFound();
        }
        else
        {
          throw;
        }
      }

      return NoContent();
    }

    // POST: BuyProducts
    [HttpPost("create")]
    public async Task<ActionResult<BuyProduct>> PostBuyProduct(BuyProduct buyProduct)
    {
      var products = _context.Products.Find(buyProduct.ProductId) ;

      if (products.Stock < buyProduct.Quantity)
      {
        return NotFound();
      }

      buyProduct.RegistrationDate = DateTime.Now;
      buyProduct.UpdateDate = DateTime.Now;
      buyProduct.Total = buyProduct.Quantity * buyProduct.UnitaryValue;

      _context.BuyProducts.Add(buyProduct);
      await _context.SaveChangesAsync();

      products.Stock -= buyProduct.Quantity;

      _context.Products.Update(products);
      await _context.SaveChangesAsync();

      return CreatedAtAction("GetBuyProduct", new { id = buyProduct.BuyProductId }, buyProduct);
    }

    // DELETE: api/BuyProducts/5
    [HttpDelete("{id}")]
    public async Task<ActionResult<BuyProduct>> DeleteBuyProduct(int id)
    {
      var buyProduct = await _context.BuyProducts.FindAsync(id);
      if (buyProduct == null)
      {
        return NotFound();
      }

      _context.BuyProducts.Remove(buyProduct);
      await _context.SaveChangesAsync();

      return buyProduct;
    }

    private bool BuyProductExists(int id)
    {
      return _context.BuyProducts.Any(e => e.BuyProductId == id);
    }
  }
}
