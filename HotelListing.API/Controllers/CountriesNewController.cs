﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelListing.API.Data;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OData.Query;
using HotelListing.API.Core.Contracts;
using HotelListing.API.Core.Models;
using HotelListing.API.Core.Models.Country;

namespace HotelListing.API.Controllers
{
    [Route("/v{version:apiVersion}/countries")]  //api/v1/countries
    [ApiController]
    //[ApiVersion("1.0", Deprecated = true)] //means no longer supported or developed
    //[Authorize]
    public class CountriesNewController : ControllerBase
    {
       
        private readonly IMapper _mapper;
        private readonly ICountriesRepository _countriesRepository;
        private readonly ILogger<CountriesNewController> _logger;

        public CountriesNewController( IMapper mapper, ICountriesRepository countriesRepository, ILogger<CountriesNewController> logger)
        {
           
            _mapper = mapper;
            this._countriesRepository = countriesRepository;
            this._logger = logger;
        }

        // GET: api/Countries 
        //Look up Odata Documentation
        [HttpGet]
        [EnableQuery]  //Anotation is for Filtering with OData use==> $Select as Key and field/Column Name e.g Name as Value on Postman e,g $Select name,ShortName, (it selects those two fields on Postman) 
        public async Task<ActionResult<IEnumerable<GetCountryDto>>> GetCountries()
        {
            var countries = await _countriesRepository.GetAllAsync();
            var records = _mapper.Map<List<GetCountryDto>>(countries);
            return Ok(records);
        }
        //GET :api/Coutries/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CountryDto>> GetCountry(int id)
        {
            //gets country and include hotels as well as one record, create its own method in its specific repository
           
            var country = await _countriesRepository.GetDetails(id);

            if (country == null)
            {
                _logger.LogWarning($"Record found in {nameof(GetCountry)} with I: {id}. ");
                return NotFound();
            }

            var countryDto = _mapper.Map<CountryDto>(country);

            return Ok(countryDto);
        }

        // PUT: api/Countries/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> PutCountry(int id, updateCountryDto updateCountryDto)
        {
            if (id != updateCountryDto.Id)
            {
                //400
                return BadRequest("Invalid Record Id");
            }

            var country = await _countriesRepository.GetAsync(id);

            if (country == null)
            {
                return NotFound();
            }

            //entity framework tracks it here where the mapping is done not need foe state.Modified here
            _mapper.Map(updateCountryDto, country);

            try
            {
                await _countriesRepository.UpdateAsync(country);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await CountryExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(updateCountryDto);
            //return NoContent();
        }

        // POST: api/Countries
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Authorize(Roles = "Administrator,User")]
        public async Task<ActionResult<Country>> PostCountry(createCountryDto createCountryDto)
        {
            
            var country = _mapper.Map<Country>(createCountryDto);

            await _countriesRepository.AddAsync(country);
        
            return CreatedAtAction("GetCountry", new { id = country.Id }, country);
        }

        // DELETE: api/Countries/5
        [HttpDelete("{id}")]
        [Authorize(Roles ="Administrator")]
        public async Task<IActionResult> DeleteCountry(int id)
        {
            var country = await _countriesRepository.GetAsync(id);
            if (country == null)
            {
                return NotFound();
            }

            await _countriesRepository.DeleteAsync(id);
            
            return NoContent();
        }

        private async Task<bool> CountryExists(int id)
        {
            return await _countriesRepository.Exists(id);
        }
    }
}
