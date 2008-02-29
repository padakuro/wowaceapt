using System;
using System.Collections.Generic;

using WowAce.AptCore;

namespace WowAce.AptGet
{
    partial class Program
    {
        public void RemoveStatusMessage(string[] message)
        {
            switch (message[0])
            {
                case "remove": Output.Append(String.Format("\nUninstalling {0}...", message[1])); break;

                case "clear.failed":
                    Output.Append(" failed.");
                    break;

                case "clear.success":
                    Output.Append(" done.");
                    break;
            }
        }
        
        public void DoRemove()
        {
            AddonLocal = new AptLocal(AddonEnv);

            Output.Info("Action: remove addon(s).");
            Output.Info(String.Format("Addons currently installed: {0}", AddonLocal.GetAddonNum()));

            if (InitializeRepository())
            {
                AptActionRemove remove = new AptActionRemove(AddonEnv, AddonLocal, AddonRemote, AddonRepo);
                remove.AddStatusListener(new AptAction.StatusMessageEventHandler(RemoveStatusMessage));

                foreach (string addon in ArgAddons)
                {
                    remove.Add(addon);
                }

                // check if exists
                List<string> queue = remove.GetQueue();
                List<string> removeList = new List<string>();
                foreach (string addon in queue)
                {
                    removeList.Add(AddonLocal.GetAddonInfo(addon).Name);
                }

                // no addons to be removed
                if (removeList.Count == 0)
                {
                    Output.Error("No addons found matching given names.");
                    return;
                }

                Output.Info("The following addons will be removed:\n\n");
                Output.Append("  " + String.Join(", ", removeList.ToArray()));

                if (!Cfg.SilentMode)
                {
                    Output.Append("\n\nDo you want to proceed? [Y/N]: ");

                    if (Convert.ToChar(Console.Read()).ToString().ToLower() == "y")
                    {
                        remove.Run();
                    }
                }
                else
                {
                    remove.Run();
                }
            }
        }
    }
}
