using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RandomMaze;
using System.Threading;

namespace fCraft {
    internal static class Economy {
        internal static void Init () {
            CommandManager.RegisterCommand( CdEconomy );
            CommandManager.RegisterCommand( CdPay );
            CommandManager.RegisterCommand( CdStore);
        }


#region Pay

               static readonly CommandDescriptor CdPay = new CommandDescriptor
        {
            Name = "Pay",
            Aliases = new[] { "Purchase" },
            Category = CommandCategory.Moderation,
            IsConsoleSafe = false,
            Permissions = new[] { Permission.Economy },
            Usage = "/pay player amount",
            Help = "&SUsed to pay a certain player an amount of bits.",
            Handler = PayHandler
        };

        static void PayHandler(Player player, Command cmd)
        {
            string targetName = cmd.Next();
            string money = cmd.Next();
            int amount;

            if (money == null)
            {
                player.Message("&ePlease select the amount of bits you wish to send.");
                return;
            }

            Player target = Server.FindPlayerOrPrintMatches(player, targetName, false, true);
            if (target == null)
            {
                player.Message("&ePlease select a player to pay bits towards.");
                return;
            }

            if (!int.TryParse(money, out amount))
            {
                player.Message("&ePlease select from a whole number.");
                return;
            }

            PayHandler(player, new Command("/economy pay " + target + " " + money));
        }
#endregion


#region Economy
        static readonly CommandDescriptor CdEconomy = new CommandDescriptor
        {
            Name = "Economy",
            Aliases = new[] { "Money", "Econ" },
            Category = CommandCategory.Moderation,
            IsConsoleSafe = true,
            Permissions = new[] { Permission.Economy },
            Usage = "/Economy [pay/give/take/show] [playername] [pay/take/give: amount]",
            Help = "&SEconomy commands. &a/Show &ewill show you the amount of money a player has," +
            " &a/Pay &ewill pay that player an amount of bits," +
            "and &a/Give &e+ &a/Take &ewill give/take coins from or to a player.",
            Handler = EconomyHandler
        };

        static void EconomyHandler(Player player, Command cmd)
        {
            try
            {
                string option = cmd.Next();
                string targetName = cmd.Next();
                string amount = cmd.Next();
                int amountnum;
                if (option == null)
                {
                    CdEconomy.PrintUsage(player);
                }
                if (option == "give")
                {
                    if (!player.Can(Permission.ManageEconomy))
                    {
                        player.Message("&cYou do not have permission to use that command!");
                        return;
                    }
                    if (targetName == null)
                    {
                        player.Message("&ePlease type in a player's name to give coins to.");
                        return;
                    }
                    Player target = Server.FindPlayerOrPrintMatches(player, targetName, false, true);

                    if (target == null)
                    {
                        return;
                    }
                    else
                    {
                        if (!int.TryParse(amount, out amountnum))
                        {
                            player.Message("&eThe amount must be a number without any decimals!");
                            return;
                        }
                        if (cmd.IsConfirmed)
                        {
                            //actually give the player the money
                            int tNewMoney = target.Info.Money + amountnum;

                            if (amountnum == 1)
                            {
                                player.Message("&eYou have given {0} &C{1} &ecoin.", target.ClassyName, amountnum);
                                target.Message("&e{0} &ehas given you {1} &ecoin.", player.ClassyName, amountnum);
                                Server.Players.Except(target).Except(player).Message("&e{0} &ewas given {1} &ecoin by {2}&e.", target.ClassyName, amountnum, player.ClassyName);
                            }
                            else
                            {
                                player.Message("&eYou have given {0} &C{1} &ecoins.", target.ClassyName, amountnum);
                                target.Message("&e{0} &ehas given you {1} &ecoins.", player.ClassyName, amountnum);
                                Server.Players.Except(target).Except(player).Message("&e{0} &ewas given {1} &ecoins by {2}&e.", target.ClassyName, amountnum, player.ClassyName);
                            }

                            target.Info.Money = tNewMoney;
                            return;
                        }
                        else
                        {
                            if (amountnum == 1) {
                            player.Confirm(cmd, "&eAre you sure you want to give {0} &C{1} &ecoin?", target.ClassyName, amountnum);
                            return;
                        }
                        else {
                            player.Confirm(cmd, "&eAre you sure you want to give {0} &C{1} &ecoins?", target.ClassyName, amountnum);
                            return;
                        }
                        }

                    }
                }
                if (option == "take")
                {
                    if (!player.Can(Permission.ManageEconomy))
                    {
                        player.Message("&cYou do not have permission to use that command.");
                        return;
                    }
                    if (targetName == null)
                    {
                        player.Message("&ePlease type in a player's name to take coins away from.");
                        return;
                    }
                    Player target = Server.FindPlayerOrPrintMatches(player, targetName, false, true);

                    if (target == null)
                    {
                        return;
                    }
                    else
                    {
                        if (!int.TryParse(amount, out amountnum))
                        {
                            player.Message("&eThe amount must be a number!");
                            return;
                        }

                        if (cmd.IsConfirmed)
                        {
                            if (amountnum > target.Info.Money)
                            {
                                player.Message("{0}&e doesn't have that many coins!", target.ClassyName);
                                return;
                            }
                            else
                            {
                                //actually give the player the money
                                int tNewMoney = target.Info.Money - amountnum;
                                if (amountnum == 1)
                                {
                                    player.Message("&eYou have taken &c{1}&e coin from {0}.", target.ClassyName, amountnum);
                                    target.Message("&e{0} &ehas taken {1} &ecoin from you.", player.ClassyName, amountnum);
                                    Server.Players.Except(target).Except(player).Message("&e{0} &etook {1} &ecoin from {2}&e.", player.ClassyName, amountnum, target.ClassyName);
                                }
                                else
                                {
                                    player.Message("&eYou have taken &c{1}&e coins from {0}.", target.ClassyName, amountnum);
                                    target.Message("&e{0} &ehas taken {1} &ecoins from you.", player.ClassyName, amountnum);
                                    Server.Players.Except(target).Except(player).Message("&e{0} &etook {1} &ecoins from {2}&e.", player.ClassyName, amountnum, target.ClassyName);
                                }
                                target.Info.Money = tNewMoney;
                                return;
                            }
                        }
                        else
                        {
                            player.Confirm(cmd, "&eAre you sure you want to take &c{1} &ecoins from {0}?", target.ClassyName, amountnum);
                            return;
                        }

                    }


                }
                if (option == "pay")
                {
                    //lotsa idiot proofing in this one ^.^

                    if (targetName == null)
                    {
                        player.Message("&ePlease type in a player's name to pay.");
                        return;
                    }
                    Player target = Server.FindPlayerOrPrintMatches(player, targetName, false, true);
                    if (target == player)
                    {
                        player.Message("You cannot pay youself.");
                        return;
                    }

                    if (target == null)
                    {
                        return;
                    }
                    else
                    {
                        if (!int.TryParse(amount, out amountnum))
                        {
                            player.Message("&eThe amount must be a number!");
                            return;
                        }

                        if (cmd.IsConfirmed)
                        {
                            if (amountnum > player.Info.Money)
                            {
                                player.Message("You don't have enough coins!");
                                return;
                            }
                            else
                            {
                                //show him da monai
                                int pNewMoney = player.Info.Money - amountnum;
                                int tNewMoney = target.Info.Money + amountnum;
                                if (amountnum == 1)
                                {
                                    player.Message("&eYou have paid &C{1}&e coin to {0}.", target.ClassyName, amountnum);
                                    target.Message("&e{0} &ehas paid you {1} &ecoin.", player.ClassyName, amountnum);
                                    Server.Players.Except(target).Except(player).Message("&e{0} &ewas paid {1} &ecoin from {2}&e.", target.ClassyName, amountnum, player.ClassyName);
                                }
                                else
                                {
                                    player.Message("&eYou have paid &C{1}&e coins to {0}.", target.ClassyName, amountnum);
                                    target.Message("&e{0} &ehas paid you {1} &ecoins.", player.ClassyName, amountnum);
                                    Server.Players.Except(target).Except(player).Message("&e{0} &ewas paid {1} &ecoins from {2}&e.", target.ClassyName, amountnum, player.ClassyName);
                                }
                                player.Info.Money = pNewMoney;
                                target.Info.Money = tNewMoney;
                                return;
                            }
                        }
                        else
                        {
                            player.Confirm(cmd, "&eAre you sure you want to pay {0}&e {1} &ecoins? Type /ok to continue.", target.ClassyName, amountnum);
                            return;
                        }


                    }
                }


                else if (option == "show")
                {

                    if (targetName == null)
                    {
                        player.Message("&ePlease type in a player's name to see how many coins they have.");
                        return;
                    }
                    Player target = Server.FindPlayerOrPrintMatches(player, targetName, false, true);

                    if (target == null)
                    {
                        return;
                    }
                    else
                    {
                        //actually show how much money that person has
                        player.Message("&e{0} has &C{1} &ecoins!", target.ClassyName, target.Info.Money);
                    }

                }
                else
                {
                    player.Message("&eOptions are &a/Economy pay&e, &a/Economy take&e, &a/Economy give&e, and &a/Economy show&e.");
                    return;
                }
            }
            catch (ArgumentNullException)
            {
                CdEconomy.PrintUsage(player);
            }
        }

#endregion


#region Store

        static readonly CommandDescriptor CdStore = new CommandDescriptor
        {
            Name = "Store",
            Aliases = new[] { "str" },
            Category = CommandCategory.Moderation,
            IsConsoleSafe = false,
            Permissions = new[] { Permission.Economy },
            Usage = "/Store [items/buy] [item] [field]",
            Help = "Buy ingame things such as a custom name."
        };

        static void StoreHandler(Player player, Command cmd)
        {
            try
            {
                string option = cmd.Next();
                string item = cmd.Next();
                string field = cmd.Next();
                if (option == null)
                {
                    CdStore.PrintUsage(player);
                }
                if (option == "items")
                    {
                    if (!player.Can(Permission.Economy))
                    {
                        player.Message("&cYou do not have permission to use that command!");
                        return;
                    }
                    if (option == null)
                    {
                        player.Message("&ePlease select an option [items/buy]");
                        return;
                    }