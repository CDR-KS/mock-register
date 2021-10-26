using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CDR.Register.Repository.Entities;
using CDR.Register.Repository.Infrastructure;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Xunit;
using Microsoft.Extensions.Configuration;
using CDR.Register.IntegrationTests.Models;
using System.Collections.Generic;
using System;

#nullable enable

namespace CDR.Register.IntegrationTests.API.Discovery
{
    /// <summary>
    /// Integration tests for GetDataRecipients.
    /// </summary>
    public class GetDataRecipients_Tests : BaseTest
    {
        private string GetExpectedDataRecipients(int? XV)
        {
            string expDRJson = string.Empty;

            try
            {
                using var dbContext = new RegisterDatabaseContext(new DbContextOptionsBuilder<RegisterDatabaseContext>().UseSqlServer(Configuration.GetConnectionString("DefaultConnection")).Options);

                var expectedDR = new
                {
                    data = dbContext.Participations.AsNoTracking()
                        .Include(participation => participation.Status)
                        .Include(participation => participation.Industry)
                        .Include(participation => participation.LegalEntity)
                        .Include(participation => participation.Brands)
                        .ThenInclude(brand => brand.BrandStatus)
                        .Include(participation => participation.Brands)
                        .ThenInclude(brand => brand.SoftwareProducts)
                        .ThenInclude(softwareProduct => softwareProduct.Status)
                        .Where(participation => participation.ParticipationTypeId == ParticipationTypeEnum.Dr)
                        .OrderBy(participation => participation.LegalEntityId)
                        .Select(participation => new
                        {
                            legalEntityId = participation.LegalEntityId,
                            legalEntityName = participation.LegalEntity.LegalEntityName,
                            accreditationNumber = XV >= 2 ? participation.LegalEntity.AccreditationNumber : null,
                            industry = participation.Industry.IndustryTypeCode,
                            logoUri = participation.LegalEntity.LogoUri,
                            status = participation.Status.ParticipationStatusCode,
                            dataRecipientBrands = participation.Brands.OrderBy(b => b.BrandId).Select(brand => new
                            {
                                dataRecipientBrandId = brand.BrandId,
                                brandName = brand.BrandName,
                                logoUri = brand.LogoUri,
                                softwareProducts = brand.SoftwareProducts.OrderBy(sp => sp.SoftwareProductId).Select(softwareProduct => new
                                {
                                    softwareProductId = softwareProduct.SoftwareProductId,
                                    softwareProductName = softwareProduct.SoftwareProductName,
                                    softwareProductDescription = softwareProduct.SoftwareProductDescription,
                                    logoUri = softwareProduct.LogoUri,
                                    status = softwareProduct.Status.SoftwareProductStatusCode
                                }),
                                status = brand.BrandStatus.BrandStatusCode,
                            }),
                            lastUpdated = participation.Brands.OrderByDescending(brand => brand.LastUpdated).First().LastUpdated.ToUniversalTime()
                        })
                        .ToList()
                };

                expDRJson = JsonConvert.SerializeObject(expectedDR,
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        Formatting = Formatting.Indented
                    });

                // SoftwarProduct order returned from CDR.Register.Repository > RegisterDiscoveryRepository > GetDataRecipientsAsync()
                // DOES NOT match the order returned above.
                // Work around is to convert the json serialised string to type ENTITY, then set the SoftwareProducts order by
                // then SERIALISE back to Json string.
                List<DataRecipient> modifiedExpDREntity = new();
                ExpectedDataRecipients? expDREntity = JsonConvert.DeserializeObject<ExpectedDataRecipients>(expDRJson);

                if (expDREntity != null)
                {
                    for (int i = 0; i < expDREntity.Data.Count; i++)
                    {
                        DataRecipient dr = new();
                        dr = expDREntity.Data[i];
                        if (dr.DataRecipientBrands.Count > 0)
                        {
                            List<DataRecipientBrand> drBrands = new();
                            drBrands = dr.DataRecipientBrands;
                            if (drBrands.Count > 0)
                            {
                                drBrands = drBrands.OrderBy(b => b.DataRecipientBrandId).ToList();
                                drBrands.ToList().ForEach(b =>
                                {
                                    b.SoftwareProducts = b.SoftwareProducts.OrderBy(sp => sp.SoftwareProductId).ToList();
                                });
                            }
                            if (!string.IsNullOrEmpty(dr.AccreditationNumber))
                            {
                                modifiedExpDREntity.Add(new DataRecipient
                                {
                                    AccreditationNumber = dr.AccreditationNumber,
                                    LegalEntityId = dr.LegalEntityId,
                                    LegalEntityName = dr.LegalEntityName,
                                    Industry = dr.Industry,
                                    LogoUri = dr.LogoUri,
                                    Status = dr.Status,
                                    DataRecipientBrands = drBrands,
                                    LastUpdated = dr.LastUpdated
                                });
                            }
                            else
                            {
                                modifiedExpDREntity.Add(new DataRecipient
                                {
                                    LegalEntityId = dr.LegalEntityId,
                                    LegalEntityName = dr.LegalEntityName,
                                    Industry = dr.Industry,
                                    LogoUri = dr.LogoUri,
                                    Status = dr.Status,
                                    DataRecipientBrands = drBrands,
                                    LastUpdated = dr.LastUpdated
                                });
                            }
                        }
                    }

                    expDREntity = new();
                    expDREntity.Data = modifiedExpDREntity;

                    expDRJson = JsonConvert.SerializeObject(expDREntity,
                        new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore,
                            Formatting = Formatting.Indented
                        });
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error getting expected data recipients");
            }
            return expDRJson;
        }

        [Theory]
        [InlineData(null, "1")]
        [InlineData(1, "1")] 
        [InlineData(2, "2")] 
        public async Task Get_ShouldRespondWith_200OK_DataRecipientsStatus(int? XV, string expectedXV)
        {
            // Arrange 
            var expectedDataRecipients = GetExpectedDataRecipients(XV);

            // Act
            var response = await new Infrastructure.API
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/banking/data-recipients",
                XV = XV?.ToString()
            }.SendAsync();

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                // Assert - Check content type
                Assert_HasContentType_ApplicationJson(response.Content);

                // Assert - Check XV
                Assert_HasHeader(expectedXV, response.Headers, "x-v");

                // Assert - Check json
                await Assert_HasContent_Json(expectedDataRecipients, response.Content);
            }
        }

        [Theory]
        [InlineData("foo")]
        public async Task Get_WithInvalidIndustry_ShouldRespondWith_400BadRequest_ErrorResponse(string industry)
        {
            // Act
            var response = await new Infrastructure.API
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/{industry}/data-recipients",
                XV = "1"
            }.SendAsync();

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                // Assert - Check content type
                Assert_HasContentType_ApplicationJson(response.Content);

                // Assert - Check error response
                var expectedContent = @"
                {
                ""errors"": [
                        {
                        ""code"": ""Field/InvalidIndustry"",
                        ""title"": ""Invalid Industry"",
                        ""detail"": """",
                        ""meta"": {}
                        }
                    ]
                }";
                await Assert_HasContent_Json(expectedContent, response.Content);
            }
        }

        [Theory]
        [InlineData("foo")]
        [InlineData("3")]
        public async Task Get_UnsupportedXV_ShouldRespondWith_406NotAcceptable_ErrorResponse(string XV)
        {
            // Act
            var response = await new Infrastructure.API
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/banking/data-recipients",
                XV = XV
            }.SendAsync();

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(HttpStatusCode.NotAcceptable);

                // Assert - Check content type
                Assert_HasContentType_ApplicationJson(response.Content);

                // Assert - Check error response
                var expectedContent = @"
                {
                ""errors"": [
                    {
                    ""code"": ""Header/UnsupportedVersion"",
                    ""title"": ""Unsupported Version"",
                    ""detail"": """",
                    ""meta"": {}
                    }
                ]
                }";
                await Assert_HasContent_Json(expectedContent, response.Content);
            }
        }

        [Theory]
        [InlineData(null)] 
        [InlineData("foo")] 
        public async Task Get_WithIfNoneMatch_ShouldRespondWith_200OK_ETag(string? ifNoneMatch)
        {
            // Arrange 
            var expectedDataRecipients = GetExpectedDataRecipients(1);

            // Act
            var response = await new Infrastructure.API
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/banking/data-recipients",
                XV = "1",
                IfNoneMatch = ifNoneMatch,
            }.SendAsync();

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                // Assert - Check content type
                Assert_HasContentType_ApplicationJson(response.Content);

                // Assert - Check XV
                Assert_HasHeader("1", response.Headers, "x-v");

                // Assert - Check has any ETag
                Assert_HasHeader(null, response.Headers, "ETag");

                // Assert - Check json
                await Assert_HasContent_Json(expectedDataRecipients, response.Content);
            }
        }

        [Fact]
        public async Task Get_WithIfNoneMatchKnownETAG_ShouldRespondWith_304NotModified_ETag()
        {
            // Arrange - Get SoftwareProductsStatus and save the ETag
            var expectedETag = (await new Infrastructure.API
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/banking/data-recipients",
                XV = "1",
                IfNoneMatch = null, // ie If-None-Match is not set                
            }.SendAsync()).Headers.GetValues("ETag").First().Trim('"');

            // Act
            var response = await new Infrastructure.API
            {
                HttpMethod = HttpMethod.Get,
                URL = $"{TLS_BaseURL}/cdr-register/v1/banking/data-recipients",
                XV = "1",
                IfNoneMatch = expectedETag,
            }.SendAsync();

            // Assert
            using (new AssertionScope())
            {
                // Assert - Check status code
                response.StatusCode.Should().Be(HttpStatusCode.NotModified);

                // Assert - No content
                (await response.Content.ReadAsStringAsync()).Should().BeNullOrEmpty();
            }
        }
    }
}
