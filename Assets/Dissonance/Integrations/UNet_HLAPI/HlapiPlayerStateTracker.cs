using UnityEngine.Networking;

namespace Dissonance.Integrations.UNet_HLAPI
{
    public class HlapiPlayerStateTracker
        : NetworkBehaviour
    {
        private static readonly Log _log = Logs.Create(LogCategory.Network, typeof(HlapiPlayerStateTracker).Name);

        private HlapiConn _peer;
        private HlapiServer _server;

        public override void OnNetworkDestroy()
        {
            _log.Debug("Stopped tracking player state ({0})", netId.Value);

            if (_server != null)
                _server.PlayerDisconnected(_peer);

            base.OnNetworkDestroy();
        }

        public override void OnStartServer()
        {
            _log.Debug("Tracking player state ({0})", netId.Value);

            base.OnStartServer();
        }

        public void Track(HlapiServer server, HlapiConn peer)
        {
            _server = server;
            _peer = peer;
        }
    }
}
