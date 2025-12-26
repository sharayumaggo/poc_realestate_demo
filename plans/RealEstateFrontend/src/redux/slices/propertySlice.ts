import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import axios from 'axios';

export enum PropertyType {
  House = 'House',
  Apartment = 'Apartment',
  Condo = 'Condo',
  Townhouse = 'Townhouse',
  Land = 'Land'
}

export enum PropertyStatus {
  Active = 'Active',
  Sold = 'Sold',
  Rented = 'Rented',
  PendingApproval = 'PendingApproval'
}

export interface Property {
  id: string;
  title: string;
  description: string;
  price: number;
  location: string;
  latitude: number;
  longitude: number;
  propertyType: PropertyType;
  bedrooms: number;
  bathrooms: number;
  landSize?: number;
  status: PropertyStatus;
  sellerId: string;
  agentId?: string;
  images: string[];
  createdAt: string;
  updatedAt: string;
}

interface PropertyState {
  properties: Property[];
  loading: boolean;
  error: string | null;
}

const initialState: PropertyState = {
  properties: [],
  loading: false,
  error: null,
};

export const fetchProperties = createAsyncThunk(
  'properties/fetchProperties',
  async () => {
    const response = await axios.get<Property[]>('http://localhost:5181/api/properties');
    return response.data;
  }
);

const propertySlice = createSlice({
  name: 'properties',
  initialState,
  reducers: {},
  extraReducers: (builder) => {
    builder
      .addCase(fetchProperties.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchProperties.fulfilled, (state, action) => {
        state.loading = false;
        state.properties = action.payload;
      })
      .addCase(fetchProperties.rejected, (state, action) => {
        state.loading = false;
        state.error = action.error.message || 'Failed to fetch properties';
      });
  },
});

export default propertySlice.reducer;