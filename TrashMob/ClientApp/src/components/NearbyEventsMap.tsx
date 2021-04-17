﻿import { useEffect, useState } from 'react';
import * as React from 'react';
import * as MapStore from '../store/MapStore'
import {
    AzureMap,
    AzureMapDataSourceProvider,
    AzureMapFeature,
    AzureMapLayerProvider,
    AzureMapsProvider,
    IAzureDataSourceChildren,
    IAzureMapFeature,
    IAzureMapLayerType,
} from 'react-azure-maps';
import { data, SymbolLayerOptions } from 'azure-maps-control';
import { FetchEventDataState } from './Home';
import EventData from './Models/EventData';

const renderPoint = (coordinates: data.Position): IAzureMapFeature => {
    const rendId = Math.random();

    return (
        <AzureMapFeature
            key={rendId}
            id={rendId.toString()}
            type="Point"
            coordinate={coordinates}
            properties={{
                title: 'Pin',
                icon: 'pin-round-blue',
            }}
        />
    );
};

const addMarkers = (eventList: EventData[]): data.Position[] => {
    return eventList.map((event) => { return new data.Position(event.latitude, event.longitude) } );
};

export interface MultipleEventMapDataState {
    eventList: EventData[];
}

const NearbyEventsMap: React.FC<MultipleEventMapDataState> = (props) => {
    const [markers, setMarkers] = useState(addMarkers(props.eventList));
    const [markersLayer] = useState<IAzureMapLayerType>('SymbolLayer');
    const [layerOptions, setLayerOptions] = useState<SymbolLayerOptions>(MapStore.memoizedOptions);
    const [isKeyLoaded, setIsKeyLoaded] = useState(false);

    useEffect(() => {
        async function getOpt() {
            MapStore.option.authOptions = await MapStore.getOption();
            setIsKeyLoaded(true);
        }
        getOpt();
    }, [])

    const memoizedMarkerRender: IAzureDataSourceChildren = React.useMemo(
        (): any => markers.map((marker) => { return renderPoint(marker); }),
        [markers],
    );

    return (
        <>
            <AzureMapsProvider>
                <div style={styles.map}>
                    {!isKeyLoaded && <div>Map is loading.</div>}
                    {isKeyLoaded && <AzureMap options={MapStore.option}>
                        <AzureMapDataSourceProvider
                            events={{
                                dataadded: (e: any) => {
                                    console.log('Data on source added', e);
                                },
                            }}
                            id={'markersExample AzureMapDataSourceProvider'}
                            options={{ cluster: true, clusterRadius: 2 }}
                        >
                            <AzureMapLayerProvider
                                id={'markersExample AzureMapLayerProvider'}
                                options={layerOptions}
                                events={{
                                    click: MapStore.clusterClicked,
                                    dbclick: MapStore.clusterClicked,
                                }}
                                lifecycleEvents={{
                                    layeradded: () => {
                                        console.log('LAYER ADDED TO MAP');
                                    },
                                }}
                                type={markersLayer}
                            />
                            {memoizedMarkerRender}
                        </AzureMapDataSourceProvider>
                    </AzureMap>}
                </div>
            </AzureMapsProvider>
        </>
    );
};

const styles = {
    map: {
        height: 300,
    },
    buttonContainer: {
        display: 'grid',
        gridAutoFlow: 'column',
        gridGap: '10px',
        gridAutoColumns: 'max-content',
        padding: '10px 0',
        alignItems: 'center',
    },
    button: {
        height: 35,
        width: 80,
        backgroundColor: '#68aba3',
        'text-align': 'center',
    },
};

export default NearbyEventsMap
