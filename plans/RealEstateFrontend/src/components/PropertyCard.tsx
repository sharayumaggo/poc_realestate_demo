import React, { useState, useEffect } from 'react';
import { Property } from '../redux/slices/propertySlice';

interface PropertyCardProps {
  property: Property;
}

const PropertyCard: React.FC<PropertyCardProps> = ({ property }) => {
  const [isFavorite, setIsFavorite] = useState(false);

  useEffect(() => {
    // Check if property is favorite - only if user is authenticated
    const token = localStorage.getItem('token');
    if (!token) return;

    const checkFavorite = async () => {
      try {
        const response = await fetch('http://localhost:5181/api/favorites', {
          headers: { Authorization: `Bearer ${token}` },
        });
        if (response.ok) {
          const favorites = await response.json();
          const isFav = favorites.some((p: Property) => p.id === property.id);
          setIsFavorite(isFav);
        }
      } catch (err) {
        console.error('Failed to check favorite status');
      }
    };

    checkFavorite();
  }, [property.id]);

  const handleFavoriteToggle = async () => {
    const token = localStorage.getItem('token');
    if (!token) return;

    try {
      if (isFavorite) {
        const response = await fetch(`http://localhost:5181/api/favorites/${property.id}`, {
          method: 'DELETE',
          headers: { Authorization: `Bearer ${token}` },
        });
        if (response.ok) {
          setIsFavorite(false);
        }
      } else {
        const response = await fetch(`http://localhost:5181/api/favorites/${property.id}`, {
          method: 'POST',
          headers: { Authorization: `Bearer ${token}` },
        });
        if (response.ok) {
          setIsFavorite(true);
        }
      }
    } catch (err) {
      console.error('Failed to toggle favorite');
    }
  };

  return (
    <div className="bg-white overflow-hidden shadow rounded-lg relative">
      <button
        onClick={handleFavoriteToggle}
        className={`absolute top-2 right-2 p-2 rounded-full ${
          isFavorite ? 'bg-red-500 text-white' : 'bg-white text-gray-400'
        } shadow-md hover:shadow-lg`}
      >
        ♥
      </button>
      <div className="h-48 bg-gray-200">
        {property.images.length > 0 ? (
          <img src={property.images[0]} alt={property.title} className="w-full h-full object-cover" />
        ) : (
          <div className="w-full h-full flex items-center justify-center text-gray-500">No Image</div>
        )}
      </div>
      <div className="p-6">
        <h3 className="text-lg font-medium text-gray-900">{property.title}</h3>
        <p className="mt-2 text-sm text-gray-500">{property.location}</p>
        <p className="mt-2 text-2xl font-bold text-gray-900">${property.price.toLocaleString()}</p>
        <div className="mt-4 flex items-center justify-between text-sm text-gray-500">
          <span>{property.bedrooms} Beds</span>
          <span>{property.bathrooms} Baths</span>
          <span>{property.propertyType}</span>
        </div>
        <button className="mt-4 w-full bg-blue-600 text-white py-2 px-4 rounded-md hover:bg-blue-700">
          View Details
        </button>
      </div>
    </div>
  );
};

export default PropertyCard;