using System.Collections;
using System.Net;
using LightJson;
using NUnit.Framework;
using Unisave.Authentication;
using Unisave.Facades;
using Unisave.Testing;
using UnisaveFixture.Backend.Authentication;
using UnisaveFixture.Backend.SteamAuthentication;
using UnisaveFixture.SteamAuthentication;
using UnityEngine;
using UnityEngine.TestTools;

namespace UnisaveFixture.Tests.SteamAuthentication
{
    public class SteamAuthenticationTest : BackendTestCase
    {
        [Test]
        public void ItRejectsLoginWithInvalidToken()
        {
            Env.Set("STEAM_API_URL", "https://partner.steam-api.com/");
            Env.Set("STEAM_PUBLISHER_KEY", "some-publisher-key");
            Env.Set("STEAM_APP_ID", "480");
            
            Http.Fake(Http.Response(new JsonObject {
                ["response"] = new JsonObject {
                    ["error"] = new JsonObject {
                        ["errorcode"] = 3,
                        ["errordesc"] = "Invalid parameter"
                    }
                }
            }));

            Assert.Catch<AuthException>(() => {
                OnFacet<SteamLoginFacet>.CallSync(
                    nameof(SteamLoginFacet.Login),
                    "some-invalid-session-token"
                );
            }, "Invalid session token.");
            
            Http.AssertSent(request =>
                request.Url == "https://partner.steam-api.com/" +
                    "ISteamUserAuth/AuthenticateUserTicket/v1/?" +
                    "key=some-publisher-key&" +
                    "appid=480&" +
                    "ticket=some-invalid-session-token"
            );
        }
        
        [Test]
        public void ItLogsPlayerIn()
        {
            var player = new PlayerEntity {
                steamId = "123456789"
            };
            player.Save();
            
            Env.Set("STEAM_API_URL", "https://partner.steam-api.com/");
            Env.Set("STEAM_PUBLISHER_KEY", "some-publisher-key");
            Env.Set("STEAM_APP_ID", "480");
            
            Http.Fake(Http.Response(new JsonObject {
                ["response"] = new JsonObject {
                    ["params"] = new JsonObject {
                        ["result"] = "OK",
                        ["steamid"] = "123456789",
                        ["ownersteamid"] = "123456789",
                        ["vacbanned"] = false,
                        ["publisherbanned"] = false
                    }
                }
            }));
            
            OnFacet<SteamLoginFacet>.CallSync(
                nameof(SteamLoginFacet.Login),
                "valid-session-token"
            );
            
            Http.AssertSent(request =>
                request.Url == "https://partner.steam-api.com/" +
                "ISteamUserAuth/AuthenticateUserTicket/v1/?" +
                "key=some-publisher-key&" +
                "appid=480&" +
                "ticket=valid-session-token"
            );
            
            Assert.IsTrue(Auth.Check());
            Assert.AreEqual(
                player.EntityId,
                Auth.Id()
            );
        }

        [Test]
        public void ItRegistersNewPlayer()
        {
            Env.Set("STEAM_API_URL", "https://partner.steam-api.com/");
            Env.Set("STEAM_PUBLISHER_KEY", "some-publisher-key");
            Env.Set("STEAM_APP_ID", "480");
            
            Http.Fake(Http.Response(new JsonObject {
                ["response"] = new JsonObject {
                    ["params"] = new JsonObject {
                        ["result"] = "OK",
                        ["steamid"] = "123456789",
                        ["ownersteamid"] = "123456789",
                        ["vacbanned"] = false,
                        ["publisherbanned"] = false
                    }
                }
            }));
            
            OnFacet<SteamLoginFacet>.CallSync(
                nameof(SteamLoginFacet.Login),
                "valid-session-token"
            );
            
            Http.AssertSent(request =>
                request.Url == "https://partner.steam-api.com/" +
                "ISteamUserAuth/AuthenticateUserTicket/v1/?" +
                "key=some-publisher-key&" +
                "appid=480&" +
                "ticket=valid-session-token"
            );

            var player = DB.First<PlayerEntity>();
            
            Assert.AreEqual("123456789", player.steamId);
            
            Assert.IsTrue(Auth.Check());
            Assert.AreEqual(
                player.EntityId,
                Auth.Id()
            );
        }
        
        [Test]
        public void LoginUpdatesTimestamp()
        {
            var player = new PlayerEntity {
                steamId = "123456789"
            };
            player.Save();
            
            Http.Fake(Http.Response(new JsonObject {
                ["response"] = new JsonObject {
                    ["params"] = new JsonObject {
                        ["result"] = "OK",
                        ["steamid"] = "123456789",
                        ["ownersteamid"] = "123456789",
                        ["vacbanned"] = false,
                        ["publisherbanned"] = false
                    }
                }
            }));
            
            OnFacet<SteamLoginFacet>.CallSync(
                nameof(SteamLoginFacet.Login),
                "valid-session-token"
            );

            var timeBefore = player.lastLoginAt;
            player.Refresh();
            var timeNow = player.lastLoginAt;
            Assert.AreNotEqual(timeBefore, timeNow);
        }
        
        [Test]
        public void YouCanLogOut()
        {
            var player = new PlayerEntity {
                steamId = "123456789"
            };
            player.Save();

            ActingAs(player);
            
            var response = OnFacet<SteamLoginFacet>.CallSync<bool>(
                nameof(SteamLoginFacet.Logout)
            );
            
            Assert.IsTrue(response);
            Assert.IsFalse(Auth.Check());
        }
        
        [Test]
        public void YouCanLogOutWhileNotLoggedIn()
        {
            var response = OnFacet<SteamLoginFacet>.CallSync<bool>(
                nameof(SteamLoginFacet.Logout)
            );
            
            Assert.IsFalse(response);
            Assert.IsFalse(Auth.Check());
        }
    }
}