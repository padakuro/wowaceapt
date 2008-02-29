using System;
using System.Collections.Generic;

using WowAce.AptCore;

namespace WowAce.AptGet
{
    partial class Program
    {
        public void DoRemove()
        {
            AddonLocal = new AptLocal(AddonEnv);

            Output.Info("Action: remove addon(s).");
            Output.Info(String.Format("Addons currently installed: {0}", AddonLocal.GetAddonNum()));

            // check if exists
            List<string> remove = new List<string>();
            foreach (string addon in ArgAddons)
            {
                if (AddonLocal.IsInstalled(addon))
                {
                    remove.Add(AddonLocal.GetAddonInfo(addon).Name);
                }
            }

            // no addons to be removed
            if (remove.Count == 0)
            {
                Output.Error("No addons found matching given names.");
                return;
            }

            Output.Info("The following addons will be removed:\n\n");
            Output.Append("  " + String.Join(", ", remove.ToArray()));

            bool proceed = false;

            if (!Cfg.SilentMode)
            {
                Output.Append("\n\nDo you want to proceed? [Y/N]: ");

                if (Convert.ToChar(Console.Read()).ToString().ToLower() == "y")
                {
                    proceed = true;
                }
            }
            else
            {
                proceed = true;
            }

            if (proceed)
            {
                foreach (string addon in remove)
                {
                    Output.Append(String.Format("\n  Uninstalling {0}...", AddonLocal.GetAddonInfo(addon).Name));
                    if (AddonLocal.Uninstall(addon))
                    {
                        Output.Append(" done.");
                    }
                    else
                    {
                        Output.Append(" failed.");
                    }
                }
            }
        }
    }
}
