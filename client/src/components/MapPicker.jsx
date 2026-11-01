import { useCallback, useEffect, useMemo } from "react";
import { MapContainer, TileLayer, Marker, Circle, useMapEvents, useMap } from "react-leaflet";
import L from "leaflet";
import markerIcon from "leaflet/dist/images/marker-icon.png";
import markerIcon2x from "leaflet/dist/images/marker-icon-2x.png";
import markerShadow from "leaflet/dist/images/marker-shadow.png";

// Vite doesn't rewrite Leaflet's default icon URLs automatically — point them at the
// bundled assets explicitly, or markers render as broken image icons.
const pinIcon = new L.Icon({
  iconUrl: markerIcon,
  iconRetinaUrl: markerIcon2x,
  shadowUrl: markerShadow,
  iconSize: [25, 41],
  iconAnchor: [12, 41],
  popupAnchor: [1, -34],
});

/**
 * A drag-and-drop map location picker — free (OpenStreetMap tiles, no API key),
 * standing in for Google Maps per the Phase 2 scope note in README.md. Click anywhere
 * on the map, or drag the pin, to set lat/lng; the shaded circle shows the attendance
 * geofence radius live as it's edited.
 */
export default function MapPicker({ lat, lng, radiusMeters, onChange, height = 260 }) {
  const position = useMemo(() => [Number(lat) || 0, Number(lng) || 0], [lat, lng]);

  const handleMove = useCallback((latlng) => {
    onChange(latlng.lat, latlng.lng);
  }, [onChange]);

  return (
    <div className="overflow-hidden rounded-xl border border-surface-border" style={{ height }}>
      <MapContainer center={position} zoom={16} style={{ height: "100%", width: "100%" }} scrollWheelZoom={true}>
        <TileLayer
          attribution='&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
          url="https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png"
        />
        <Marker
          position={position}
          icon={pinIcon}
          draggable
          eventHandlers={{ dragend: (e) => handleMove(e.target.getLatLng()) }}
        />
        <Circle center={position} radius={Number(radiusMeters) || 100} pathOptions={{ color: "#818cf8", fillColor: "#6366f1", fillOpacity: 0.12 }} />
        <ClickToMove onMove={handleMove} />
        <Recenter position={position} />
      </MapContainer>
    </div>
  );
}

function ClickToMove({ onMove }) {
  useMapEvents({
    click: (e) => onMove(e.latlng),
  });
  return null;
}

// Keep the map centered on the pin when the parent updates lat/lng from the number
// inputs (rather than from a drag), e.g. after typing coordinates directly.
function Recenter({ position }) {
  const map = useMap();
  useEffect(() => {
    map.setView(position);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [position[0], position[1]]);
  return null;
}
